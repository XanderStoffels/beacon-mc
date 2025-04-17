using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading.Channels;
using Beacon.Net.Packets;
using Beacon.Net.Packets.Handshaking.ServerBound;
using Beacon.Net.Packets.Login.ServerBound;
using Beacon.Net.Packets.Status;
using Beacon.Net.Packets.Status.ServerBound;

namespace Beacon.Net;

public sealed class Connection : IDisposable
{
    public ConnectionState State { get; private set; } = ConnectionState.Handshaking;
    public TcpClient Tcp { get; private set; }

    private readonly Server _server;
    private readonly Channel<IClientBoundPacket> _clientBoundPackets;
    private readonly ILogger<Connection> _logger;

    private bool _shouldClose;
    private bool _expectLoginAck;

    public Connection(Server server, TcpClient client, ILogger<Connection> logger)
    {
        _server = server;
        _clientBoundPackets = Channel.CreateUnbounded<IClientBoundPacket>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false
        });
        _logger = logger;
        Tcp  = client;
        
        if (!Tcp.GetStream().CanRead)
            throw new ArgumentException("TcpClient is not readable");
        
        if (!Tcp.GetStream().CanWrite)
            throw new ArgumentException("TcpClient is not writable");
    }

    public async Task SendAndReceivePackets(ChannelWriter<QueuedServerBoundPacket> serverPacketQueue, CancellationToken cancelToken)
    {
        await Task.WhenAll(
            ProcessServerBoundPackets(serverPacketQueue, cancelToken),
            ProcessClientBoundPackets(cancelToken)
        );
    }
    
    public bool EnqueuePacket(IClientBoundPacket packet)
    {
        return _clientBoundPackets.Writer.TryWrite(packet);
    }
    
    /// <summary>
    /// Starts accepting packets, putting them on the queue.
    /// </summary>
    /// <param name="packetQueue">The <see cref="ChannelWriter{IServerBoundPacket}"/> to put the packets on.</param>
    /// <param name="cancelToken"></param>
    private async Task ProcessServerBoundPackets(ChannelWriter<QueuedServerBoundPacket> packetQueue, CancellationToken cancelToken)
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
        
        await foreach (var packet in _clientBoundPackets.Reader.ReadAllAsync(cancelToken))
        {
            try
            { 
                // The payload is all the bytes after the initial VarInt.
                // Before writing the payload, we need to write the VarInt length of the payload.
                if (!packet.TryWritePayloadTo(buffer.Span, out var payloadLength))
                {
                    _logger.LogWarning("Failed to write payload of {PacketId} to buffer", packet.GetType().Name);
                    continue;
                }
                VarInt.TryWrite(prefixBuffer.Span, payloadLength, out var prefixLength);
                await stream.WriteAsync(prefixBuffer[..prefixLength], CancellationToken.None);
                await stream.WriteAsync(buffer[..payloadLength], CancellationToken.None);
                await stream.FlushAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing server bound packet");
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
                return ParseHandshakingPacket(ref reader, packetId);
            case ConnectionState.Status:
                return ParseStatusPacket(ref reader, packetId);
            case ConnectionState.Login:
                return ParseLoginPacket(ref reader, packetId);
            case ConnectionState.Transfer:
            case ConnectionState.Configuration:
            case ConnectionState.Play:
            default:
                throw new NotImplementedException($"State {State} is not implemented");
        }
    }

    private IServerBoundPacket? ParseHandshakingPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            // Handshaking
            case Handshake.PacketId:
                var handshake = Handshake.Deserialize(ref reader);
                State = (ConnectionState)handshake.NextState;
                return null;
            
            case 0xFE:
                // This is a legacy packet.
                var stream = Tcp.GetStream();
                stream.WriteByte(0xFF);
                _shouldClose = true;
                return null;
        }
        
        throw new NotImplementedException($"Parsing packet id {packetId} for state Handshaking is not implemented");
    }

    private StatusRequest? ParseStatusPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case StatusRequest.PacketId:
                return StatusRequest.Instance;
            
            case PingRequest.PacketId:
                // Instead of creating a new packet here, we can just inline the logic because it's so simple.
                reader.TryReadLong(out var timestamp);
                PingRequest.WritePong(Tcp.GetStream(), timestamp);
                
                // No further handling is needed, the packet is already sent.
                return null;
        }
        
        throw new NotImplementedException($"Parsing packet id {packetId} for state Status is not implemented");
    }

    private IServerBoundPacket? ParseLoginPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case Hello.PacketId:
                _expectLoginAck = true;
                return Hello.Deserialize(ref reader);  
            
            case 0x03 when _expectLoginAck:
                _expectLoginAck = false;
                State = ConnectionState.Configuration;
                return null;
            
            case 0x03: 
                _shouldClose = true;
                return null;
        }
        throw new NotImplementedException($"Parsing packet id {packetId} for state Login is not implemented");
    }

    public void Dispose()
    {
        Tcp.Dispose();
    }
}