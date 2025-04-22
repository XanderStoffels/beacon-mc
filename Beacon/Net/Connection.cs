using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using Beacon.Api.Plugins;
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
public sealed partial class Connection : IDisposable
{
    public ConnectionState State { get; private set; } = ConnectionState.Handshaking;
    public TcpClient Tcp { get; private set; }
    public Player? Player { get; set; }

    private readonly Server _server;
    private readonly Channel<IClientBoundPacket> _clientBoundPackets;
    private readonly ILogger<Connection> _logger;
    private readonly KeepAliver _configurationKeepAliver = new(TimeSpan.FromSeconds(15));
    private readonly FrozenDictionary<PacketIdentifier, FrozenSet<IPacketInterceptor>> _interceptors;

    private bool _shouldClose;

    public Connection(Server server, TcpClient client, ILogger<Connection> logger)
        : this(server, client, FrozenDictionary<PacketIdentifier, FrozenSet<IPacketInterceptor>>.Empty, logger)
    {
    }
    
    public Connection(Server server, TcpClient client, FrozenDictionary<PacketIdentifier, FrozenSet<IPacketInterceptor>> interceptors, ILogger<Connection> logger)
    {
        if (!client.GetStream().CanRead)
            throw new ArgumentException("TcpClient is not readable");

        if (!client.GetStream().CanWrite)
            throw new ArgumentException("TcpClient is not writable");

        _server = server;
        _interceptors = interceptors;
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
                
#if DEBUG
                // Write the buffer to a file for debugging purposes.
                var builder = new StringBuilder();
                // Write the bytes as decimal values on a single line.
              
                builder.Append(State.ToString());
                builder.Append(" < ");
                foreach (var b in prefixBuffer[..prefixLength].ToArray())
                {
                    builder.Append(b);
                    builder.Append(' ');
                }
                foreach (var b in buffer[..payloadLength].ToArray())
                {
                    builder.Append(b);
                    builder.Append(' ');
                }
                builder.AppendLine();
                Directory.CreateDirectory("packets");
                File.AppendAllText($"packets/{Tcp.Client.RemoteEndPoint}.txt", builder.ToString());
#endif
                
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
        
#if DEBUG
        // Write the buffer to a file for debugging purposes.
        var builder = new StringBuilder();
        Span<byte> bufferSpan = stackalloc byte[(int)buffer.Length];
        buffer.CopyTo(bufferSpan);
        // Write the bytes as decimal values on a single line.
        builder.Append(State.ToString());
        builder.Append(" > ");
        foreach (var b in bufferSpan)
        {
            builder.Append(b);
            builder.Append(' ');
        }
        builder.AppendLine();
        Directory.CreateDirectory("packets");
        File.AppendAllText($"packets/{Tcp.Client.RemoteEndPoint}.txt", builder.ToString());
#endif

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