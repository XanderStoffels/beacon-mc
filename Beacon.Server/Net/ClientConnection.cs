using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Beacon.Server.Logging;
using Beacon.Server.Net.Packets;
using Beacon.Server.Net.Packets.Exceptions;
using Beacon.Server.Net.Packets.Handshaking.Serverbound;
using Beacon.Server.Net.Packets.Status.ServerBound;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Net;

public sealed class ClientConnection
{
    private readonly BeaconServer _server;
    private readonly ILogger _logger;
    
    public TcpClient Tcp { get; }

    public EndPoint? RemoteEndPoint => Tcp.Client.RemoteEndPoint;
    public string? Ip => RemoteEndPoint?.ToString();
    public ConnectionState State { get; internal set; }
    public bool ExpectLegacyPing { get; set; }
    public NetworkStream NetworkStream => Tcp.GetStream();

    

    public ClientConnection(TcpClient tcp, BeaconServer server, ILogger logger)
    {
        Tcp = tcp;
        _server = server;
        _logger = logger;
        State = ConnectionState.Handshaking;
        ExpectLegacyPing = true;
    }

    public async Task AcceptPacketsAsync(CancellationToken cancelToken)
    {
        var reader = PipeReader.Create(NetworkStream);
        _logger.LogDebug("[{IP}] [{State}] Start accepting packets", Ip, State);
        await ReadPipeAsync(reader);
        _logger.LogDebug("[{IP}] [{State}] Stopped accepting packets", Ip, State);
    }
    
    private async Task ReadPipeAsync(PipeReader pipeReader)
    {
        var keepReading = true;
        while (keepReading)
        {
            var readResult = await pipeReader.ReadAsync();
            if (readResult.IsCanceled)
            {
                _logger.LogPipeCanceledFromWriter(Ip, State);
                return;
            }
            
            var buffer = readResult.Buffer;

            // Keep processing this buffer until we require more data from the pipe.
            // We handle this in a sync method so we can parse the buffer using a SequenceReader.
            // No async needed since all bytes are already in memory.
            while (ParsePacketsWhileEnoughData())
            {}
            
            // True: we have enough data. Process some more. False: wait for more data.
            bool ParsePacketsWhileEnoughData()
            {
                var sequenceReader = new SequenceReader<byte>(buffer);
                // Try to check if there are enough bytes to read a VarInt.
                if (!sequenceReader.TryReadVarInt(out var nextPacketSize, out _))
                    return false; // We don't have enough data to read a VarInt. Wait for more data.

                // We now know the size of the packet. 
                // Check if the reader has enough data for the whole packet.
                if (!sequenceReader.TryReadExact(nextPacketSize, out var packetData))
                    return false; // There is not enough data for the whole packed. Wait some more.

                var parseResult = ParseAndQueuePacket(packetData);
                switch (parseResult)
                {
                    case QueueAndParseResult.Ok:
                        buffer = buffer.Slice(nextPacketSize + 1);
                        return !buffer.IsEmpty;
                    case QueueAndParseResult.NeedMoreData:
                        return false;
                    case QueueAndParseResult.InvalidPacket:
                        throw new PacketParsingException($"Invalid packet received from {Ip}"); ;
                    case QueueAndParseResult.CouldNotQueue:
                        throw new($"Could not queue a packet for {Ip}");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parseResult));
                }
                
            }
            
            pipeReader.AdvanceTo(buffer.Start, buffer.End);
            
            // Is there any more data coming after this?
            keepReading = !readResult.IsCompleted;
        }
    }

    private QueueAndParseResult ParseAndQueuePacket(ReadOnlySequence<byte> sequence)
    {
        // Get the packet ID.
        var reader = new SequenceReader<byte>(sequence);
        if (!reader.TryReadVarInt(out var packetId, out _))
            throw new("Could not parse packet ID");
        
        var (packet, parseResult) =  State switch
        {
            ConnectionState.Handshaking => ParseHandshakeStatePacket(packetId, ref reader),
            ConnectionState.Status => ParseStatusStatePacket(packetId, ref reader),
            ConnectionState.Login => ParseLoginStatePacket(packetId, ref reader),
            ConnectionState.Play => ParsePlayStatePacket(packetId, ref reader),
            _ => throw new ArgumentOutOfRangeException(nameof(State))
        };

        if (packet is null)
            return parseResult;
 
        if (_server.IncomingPacketsChannel.TryWrite(new(this, packet)))
        {
            _logger.LogDebug("[{IP}] [{State}] Packet with ID {PacketId} queued", Ip, State, packet.Id);
            return parseResult;
        }
        
        _logger.LogWarning("[{IP}] [{State}] Unable to write parsed packet (id: {PacketId}) to packet channel", Ip, State, packet.Id);
        return QueueAndParseResult.CouldNotQueue;

    }
    

    private (IServerBoundPacket? Packet, QueueAndParseResult Result) ParseHandshakeStatePacket(int packetId, ref SequenceReader<byte> reader)
    {
        switch (packetId)
        {
            case 0x00: // Handshake
                if (!HandshakePacket.TryRentAndFill(ref reader, out var handshake)) 
                    return (null, QueueAndParseResult.NeedMoreData);
                
                State = handshake?.NextState ?? State;
                return (handshake, QueueAndParseResult.Ok);

            case 0xFE when ExpectLegacyPing:
                _logger.LogWarning("[{IP}] [{State}] Received unsupported legacy ping packet", Ip, State);
                ExpectLegacyPing = false;
                return (null, QueueAndParseResult.InvalidPacket);
                
            default:
                _logger.LogWarning("[{IP}] [{State}] Packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return (null, QueueAndParseResult.InvalidPacket);
        }
    }
    private (IServerBoundPacket? Packet, QueueAndParseResult Result) ParseStatusStatePacket(int packetId, ref SequenceReader<byte> reader)
    {
        IServerBoundPacket? packet;
        switch (packetId)
        {
            case 0x00: // Status Request
                return StatusRequestPacket.TryRentAndFill(ref reader, out packet)
                    ? (packet, QueueAndParseResult.Ok)
                    : (null, QueueAndParseResult.NeedMoreData);

            case 0xFE when ExpectLegacyPing: // Legacy client server list ping.
                // This can happen in in this state in the event of a malformed server list pong packet that has been sent to the client.
                ExpectLegacyPing = false;
                _logger.LogInformation("[{IP}] [{State}] Received unsupported legacy ping packet", Ip, State);
                return (null, QueueAndParseResult.InvalidPacket);
            
            case 0x01: // Ping Request
                return PingRequestPacket.TryRentAndFill(ref reader, out packet)
                    ? (packet, QueueAndParseResult.Ok)
                    : (null, QueueAndParseResult.NeedMoreData);
            
            default:
                ExpectLegacyPing = false;
                _logger.LogWarning("[{IP}] [{State}] Packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return (null, QueueAndParseResult.InvalidPacket);
        } 
        
    }
    private (IServerBoundPacket? Packet, QueueAndParseResult Result) ParseLoginStatePacket(int packetId, ref SequenceReader<byte> reader)
    {
        switch (packetId)
        {
            case 0x00: // Login Start
            case 0x01: // Encryption Response
            case 0x02: // Login Plugin Response
                _logger.LogWarning("[{IP}] [{State}] Packet Id {PacketId} is not yet implemented", Ip, State, packetId);
                return (null, QueueAndParseResult.InvalidPacket);
            default:
                _logger.LogWarning("[{IP}] [{State}] Packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return (null, QueueAndParseResult.InvalidPacket);
        }    

    }
    private (IServerBoundPacket? Packet, QueueAndParseResult Result) ParsePlayStatePacket(int packetId, ref SequenceReader<byte> reader)
    {
        switch (packetId)
        {
            case 0x00: // Confirm Teleportation
            case 0x01: // Query Block Entity Tag
            case 0x02: // Change Difficulty
            case 0x03: // Message Acknowledgment
            case 0x04: // Chat Command
            case 0x05: // Chat Message
            case 0x06: // Client Command
            case 0x07: // Client Information
            case 0x08: // Command Suggestion Request
            case 0x09: // Click Container Button
            case 0x0A: //  Click Container
            case 0x0B: //  Close Container
            case 0x0C: //  Plugin Message
            case 0x0D: //  Edit Book
            case 0x0E: // Query Entity Tag
            case 0X0F: //  Interact Entity
            case 0x10: // Jigsaw Generate
            case 0x11: // Keep Alive
            case 0x12: // Lock Difficulty
            case 0x13: // Set Player Position
            case 0x14: // Set Player Position And Rotation
            case 0x015: // Set Player Rotation
            case 0x16: // Set On Ground
            case 0x17: // Move Vehicle
            case 0x18: // Paddle Boat.
            case 0x19: // Pick Item
            case 0x1A: // Place Recipe
            case 0x1B: // Player Abilities
            case 0x1C: // Player Action
            case 0x1D: // Player Command
            case 0x1E: // Player Input
            case 0x1F: // Pong (play)
            case 0x20: // Player Session
            case 0x21: // Change Recipe Book Settings
            case 0x22: // Set Seen Recipe
            case 0x23: // Rename Item
            case 0x24: // Resource Pack
            case 0x25: // Seen Advancements
            case 0x26: // Select Trade
            case 0x27: // Set Beacon Effect
            case 0x28: // Set Held Item
            case 0x29: // Program Command Block
            case 0x2A: // Program Command Block Minecart
            case 0x2B: // Set Create Mode Slot
            case 0x2C: // Program Jigsaw Block
            case 0x2D: // Program Structure Block
            case 0x2E: // Update Sign
            case 0x2F: // Swing Arm
            case 0x30: // Teleport To Entity
            case 0x31: // Use Item On
            case 0x32: // Use Item
                _logger.LogWarning("[{IP}] [{State}] Packet Id {PacketId} is not yet implemented", Ip, State, packetId);
                return (null, QueueAndParseResult.InvalidPacket);
            default:
                _logger.LogWarning("[{IP}] [{State}] Packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return (null, QueueAndParseResult.InvalidPacket);
        }

    }
    public void Dispose()
    {
        Tcp.Dispose();
    }
}