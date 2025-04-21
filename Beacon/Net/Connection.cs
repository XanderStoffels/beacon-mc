using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using Beacon.Core;
using Beacon.Net.Packets;
using Beacon.Net.Packets.Configuration.ClientBound;
using Beacon.Net.Packets.Configuration.ServerBound;
using Beacon.Net.Packets.Handshaking.ServerBound;
using Beacon.Net.Packets.Login.ServerBound;
using Beacon.Net.Packets.Status;
using Beacon.Net.Packets.Status.ServerBound;

namespace Beacon.Net;

[DebuggerDisplay("{State}, {Player?.Username} {Tcp.Client?.RemoteEndPoint}")]
public sealed class Connection : IDisposable
{
    public ConnectionState State { get; private set; } = ConnectionState.Handshaking;
    public TcpClient Tcp { get; private set; }
    public Player? Player { get; set; }

    private readonly Server _server;
    private readonly Channel<IClientBoundPacket> _clientBoundPackets;
    private readonly ILogger<Connection> _logger;
    private readonly KeepAliver _configurationKeepAliver = new(TimeSpan.FromSeconds(15));

    private bool _shouldClose;
    private bool _expectLoginAck;
    private bool _expectConfigurationAck;

    public Connection(Server server, TcpClient client, ILogger<Connection> logger)
    {
        if (!client.GetStream().CanRead)
            throw new ArgumentException("TcpClient is not readable");

        if (!client.GetStream().CanWrite)
            throw new ArgumentException("TcpClient is not writable");

        _server = server;
        _clientBoundPackets = Channel.CreateUnbounded<IClientBoundPacket>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false
        });
        _logger = logger;
        Tcp = client;

        _configurationKeepAliver.IntervalStarted += OnConfigurationKeepAliveIntervalStarted;
        _configurationKeepAliver.TimerExpired += OnConfigurationKeepAliveExpired;
    }

    public async Task SendAndReceivePackets(ChannelWriter<QueuedServerBoundPacket> serverPacketQueue,
        CancellationToken cancelToken)
    {
        var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
        var processInbound = ProcessServerBoundPackets(serverPacketQueue, cancelSource.Token);
        var processOutbound = ProcessClientBoundPackets(cancelSource.Token);

        await Task.WhenAny(
            processInbound,
            processOutbound
        );

        // Check why the task completed.
        if (processInbound.IsFaulted)
        {
            _logger.LogError(processInbound.Exception, "Error processing inbound packets");
            _shouldClose = true;
        }

        if (processOutbound.IsFaulted)
        {
            _logger.LogError(processOutbound.Exception, "Error processing outbound packets");
            _shouldClose = true;
        }

        await cancelSource.CancelAsync();
    }

    public bool EnqueuePacket(IClientBoundPacket packet)
    {
        if (_shouldClose)
            return false;

        if (packet is FinishConfiguration)
            _expectConfigurationAck = true;

        return _clientBoundPackets.Writer.TryWrite(packet);
    }

    private async Task ProcessServerBoundPackets(ChannelWriter<QueuedServerBoundPacket> packetQueue,
        CancellationToken cancelToken)
    {
        var stream = Tcp.GetStream();

        // This pipe handles all incoming traffic.
        // The writer is given by .NET using PipeReader.Create, this will write data from the stream into the pipeline.
        // The reader reads data from the pipe and parses it as packets.
        var reader = PipeReader.Create(stream);

        while (!cancelToken.IsCancellationRequested && !_shouldClose)
        {
            // ReadAsync will block until there is data available.
            // The result will be a buffer that contains the data read from the stream.
            // The buffer is a ReadOnlySequence<byte>, which is a memory-efficient way to handle large amounts of data.
            // It can be sliced and manipulated without copying the data.
            var result = await reader.ReadAsync(cancelToken);
            if (result.IsCanceled) return;

            var buffer = result.Buffer;


            while (TryReadPacket(ref buffer, out IServerBoundPacket? packet) && !_shouldClose)
            {
                // Write the parsed packed on a queue to be handled by the main game loop.
                if (packet is null) continue;
                var queuedPacket = new QueuedServerBoundPacket(packet, this);
                packetQueue.TryWrite(queuedPacket);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }
    }

    private async Task ProcessClientBoundPackets(CancellationToken cancelToken)
    {
        const int twoMegabytes = 2 * 1024 * 1024;

        // The prefix buffer is used to write the VarInt length of the payload. Max length is 5 bytes.
        var stream = Tcp.GetStream();
        Memory<byte> prefixBuffer = new byte[5];
        Memory<byte> buffer = new byte[twoMegabytes];

        await foreach (IClientBoundPacket packet in _clientBoundPackets.Reader.ReadAllAsync(cancelToken))
            using (packet as IDisposable)
            {
                if (_shouldClose || cancelToken.IsCancellationRequested)
                {
                    return;
                }

                // The payload is all the bytes after the initial VarInt.
                // Before writing the payload, we need to write the VarInt length of the payload.
                if (!packet.SerializePayload(buffer.Span, out var payloadLength))
                {
                    _logger.LogWarning("Failed to write payload of {PacketId} to buffer", packet.GetType().Name);
                    continue;
                }

                VarInt.TryWrite(prefixBuffer.Span, payloadLength, out var prefixLength);
                try
                {
                    await stream.WriteAsync(prefixBuffer[..prefixLength], CancellationToken.None);
                    await stream.WriteAsync(buffer[..payloadLength], CancellationToken.None);
                    await stream.FlushAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error writing a client bound packet {PacketName} to the stream",
                        packet.GetType().Name);
                }
            }
    }

    private bool TryReadPacket(ref ReadOnlySequence<byte> buffer, out IServerBoundPacket? packet)
    {
        if (buffer.Length == 0)
        {
            packet = null;
            return false;
        }

        var bufferReader = new SequenceReader<byte>(buffer);

        // The first few bytes should always be a VarInt.
        if (!bufferReader.TryReadVarInt(out var packetSize, out var packetSizeVarIntSize))
        {
            // Could not read a VarInt, wait for more data.
            packet = null;
            return false;
        }

        // We now know the size of the packet. 
        // Check if the reader has enough data for the whole packet. If not, wait some more.
        if (!bufferReader.TryReadExact(packetSize, out var packetData))
        {
            packet = null;
            return false;
        }

        // The packet reader read over the exact size of the packet.
        var packetReader = new SequenceReader<byte>(packetData);
        if (!packetReader.TryReadVarInt(out var packetId, out _))
        {
            throw new InvalidDataException("Expected a PacketId as a VarInt");
        }


        packet = ParsePacketData(ref packetReader, packetId);
        buffer = buffer.Slice(packetSizeVarIntSize + packetSize);
        return true;
    }

    private IServerBoundPacket? ParsePacketData(ref SequenceReader<byte> reader, int packetId)
    {
        switch (State)
        {
            case ConnectionState.Handshaking:
                ParseHandshakingPacket(ref reader, packetId);
                return null;
            case ConnectionState.Status:
                ParseStatusPacket(ref reader, packetId);
                return null;
            case ConnectionState.Login:
                ParseLoginPacket(ref reader, packetId);
                return null;
            case ConnectionState.Configuration:
                ParseConfigurationPacket(ref reader, packetId);
                return null;
            case ConnectionState.Transfer:
            case ConnectionState.Play:
            default:
                throw new NotImplementedException($"State {State} is not implemented");
        }
    }

    private void ParseHandshakingPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case Handshake.PacketId:
            {
                using var handshake = Handshake.Rent();
                handshake.DeserializePayload(ref reader);
                State = (ConnectionState)handshake.NextState;

                // Send a finish configuration packet to skip the configuration phase.
                var finishConfiguration = new FinishConfiguration();
                EnqueuePacket(finishConfiguration);
                return;
            }
            case 0xFE:
            {
                // This is a legacy packet.
                var stream = Tcp.GetStream();
                stream.WriteByte(0xFF);
                _shouldClose = true;
                return;
            }
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Handshaking is not implemented");
    }

    private StatusRequest? ParseStatusPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case StatusRequest.PacketId:
                // Don't clutter the game loop with this packet. It does not affect the game state.
                StatusRequest.Instance.Handle(_server, this);
                return null;

            case PingRequest.PacketId:
                // Instead of creating a new packet here, we can just inline the logic because it's so simple.
                reader.TryReadLong(out var timestamp);
                PingRequest.WritePong(Tcp.GetStream(), timestamp);

                // No further handling is needed, the packet is already sent.
                return null;
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Status is not implemented");
    }

    /// <summary>
    /// Login packets do not change the state of the world, so they are handled immediately.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="packetId"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ParseLoginPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case Hello.PacketId:
            {
                _expectLoginAck = true;
                using var hello = Hello.Rent();
                hello.DeserializePayload(ref reader);
                hello.Handle(_server, this);
                return;
            }

            case 0x03 when _expectLoginAck:
            {
                _expectLoginAck = false;
                State = ConnectionState.Configuration;

                if (!_configurationKeepAliver.IsRunning)
                    _configurationKeepAliver.Start();
                return;
            }

            case 0x03:
            {
                _shouldClose = true;
                return;
            }
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Login is not implemented");
    }

    /// <summary>
    /// Configuration packets do not change the state of the world, so they are handled immediately.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="packetId"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ParseConfigurationPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case ClientInformation.PacketId:
            {
                using var packet = ClientInformation.Rent();
                packet.DeserializePayload(ref reader);
                packet.Handle(_server, this);
                return;
            }
            case CustomPayloadFromClient.PacketId:
            {
                using var packet = CustomPayloadFromClient.Rent();
                packet.DeserializePayload(ref reader);
                packet.Handle(_server, this);
                return;
            }
            case AckFinishConfiguration.PacketId:
            {
                if (!_expectConfigurationAck)
                {
                    _logger.LogWarning($"Received configuration ack, but we didn't ask for it. Closing connection");
                    _shouldClose = true;
                    return;
                }

                State = ConnectionState.Play;
                return;
            }
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Configuration is not implemented");
    }

    #region Events

    private void OnConfigurationKeepAliveIntervalStarted(object? sender, EventArgs e)
    {
        if (State != ConnectionState.Configuration)
        {
            _configurationKeepAliver.Stop();
            return;
        }

        // EnqueuePacket(new KeepAlive());
    }

    private void OnConfigurationKeepAliveExpired(object? sender, EventArgs e)
    {
        if (State != ConnectionState.Configuration)
        {
            _configurationKeepAliver.Stop();
            return;
        }

        _logger.LogWarning("Configuration keep alive expired, closing connection");
        _shouldClose = true;
    }

    #endregion "Events"

    public void Dispose()
    {
        _configurationKeepAliver.IntervalStarted -= OnConfigurationKeepAliveIntervalStarted;
        _configurationKeepAliver.TimerExpired -= OnConfigurationKeepAliveExpired;
        _configurationKeepAliver.Dispose();
        _shouldClose = true;
        Tcp.Dispose();
    }
}