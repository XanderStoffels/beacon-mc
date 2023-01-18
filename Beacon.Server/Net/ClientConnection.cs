using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Beacon.Server.Net.Packets;
using Beacon.Server.Net.Packets.Handshaking.Serverbound;
using Beacon.Server.Net.Packets.Status.Serverbound;
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
        var pipe = new Pipe();
        var writeTask = FillPipeAsync(pipe.Writer, cancelToken);
        var readTask = ReadPipeAsync(pipe.Reader);

        _logger.LogDebug("[{IP}] [{State}] Start accepting packets", Ip, State);
        await Task.WhenAll(writeTask, readTask);
        _logger.LogDebug("[{IP}] [{State}] Stopped accepting packets", Ip, State);

    }

    private async Task FillPipeAsync(PipeWriter writer, CancellationToken cancelToken)
    {
        const int minimumBufferSize = 1024 * 2;
        while (!cancelToken.IsCancellationRequested)
        {
            var memory = writer.GetMemory(minimumBufferSize);
            try
            {
                var amountRead = await NetworkStream.ReadAsync(memory, cancelToken);
                if (amountRead == 0) // Client disconnected
                    break;
                
                writer.Advance(amountRead);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[{IP}] [{State}] Error while reading from the network stream", Ip, State);
                await writer.CompleteAsync(e); // Let the reader know there is no more data due to error.
                break;
            }
            
            var result = await writer.FlushAsync(cancelToken);
            if (result.IsCompleted) // Reader indicated that it no longer wants data.
                return;
        }

        await writer.CompleteAsync();
    }
    private async Task ReadPipeAsync(PipeReader pipeReader)
    {
        var packetChannel = _server.IncomingPacketsChannel;
        var keepReading = true;
        while (keepReading)
        {
            var readResult = await pipeReader.ReadAsync();
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

                var packet = ParsePacket(packetData);
                if (packet == null)
                {
                    _logger.LogWarning("[{IP}] [{State}] Dropped a packet because it was probably malformed or unsupported", Ip, State);
                }
                else if (!packetChannel.TryWrite(new(this, packet)))
                {
                    _logger.LogWarning("[{IP}] [{State}] Unable to write parsed packet (id: {PacketId}) to packet channel", Ip, State, packet.Id);
                }
                else
                {
                    _logger.LogDebug("[{IP}] [{State}] Packet with ID {PacketId} queued", Ip, State, packet.Id);
                }

                // The buffer now no longer contains the packet. Skip the bytes that represented the packet.
                // Buffer.Start is now the start of the next packet. Everything until Buffer.End is examined, but not consumed.
                buffer = buffer.Slice(nextPacketSize);
                return true;
            }
            
            pipeReader.AdvanceTo(buffer.Start, buffer.End);
            
            // Is there any more data coming after this?
            keepReading = !readResult.IsCompleted;
        }
    }

    private IServerBoundPacket? ParsePacket(ReadOnlySequence<byte> sequence)
    {
        // Get the packet ID.
        var reader = new SequenceReader<byte>(sequence);
        if (!reader.TryReadVarInt(out var packetId, out _))
            throw new("Could not parse packet ID");
        
        return State switch
        {
            ConnectionState.Handshaking => ParseHandshakeStatePacket(packetId, reader),
            ConnectionState.Status => ParseStatusStatePacket(packetId, reader),
            ConnectionState.Login => ParseLoginStatePacket(packetId, reader),
            ConnectionState.Play => ParsePlayStatePacket(packetId, reader),
            _ => throw new ArgumentOutOfRangeException(nameof(State))
        };
    }
    

    private IServerBoundPacket? ParseHandshakeStatePacket(int packetId, SequenceReader<byte> reader)
    {
        IServerBoundPacket? packet;
        switch (packetId)
        {
            case 0x00: // Handshake
                HandshakePacket.TryRentAndFill(reader, out packet);
                return packet;

            case 0xFE when ExpectLegacyPing:
                _logger.LogWarning("[{IP}] [{State}] Received unsupported legacy ping packet", Ip, State);
                ExpectLegacyPing = false;
                return null;
                
            default:
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return null;
        }
    }
    private IServerBoundPacket? ParseStatusStatePacket(int packetId, SequenceReader<byte> reader)
    {
        IServerBoundPacket? packet;
        switch (packetId)
        {
            case 0x00: // Status Request
                StatusRequestPacket.TryRentAndFill(reader, out packet);
                return packet;

            case 0xFE when ExpectLegacyPing: // Legacy client server list ping.
                // This can happen in in this state in the event of a malformed server list pong packet that has been sent to the client.
                ExpectLegacyPing = false;
                _logger.LogInformation("[{IP}] [{State}] Received unsupported legacy ping packet", Ip, State);
                return null;
            
            case 0x01: // Ping Request
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not yet implemented", Ip, State, packetId);
                return null;
            default:
                ExpectLegacyPing = false;
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return null;
        } 
        
    }
    private IServerBoundPacket? ParseLoginStatePacket(int packetId, SequenceReader<byte> reader)
    {
        switch (packetId)
        {
            case 0x00: // Login Start
            case 0x01: // Encryption Response
            case 0x02: // Login Plugin Response
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not yet implemented", Ip, State, packetId);
                return null;
            default:
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return null;
        }    

    }
    private IServerBoundPacket? ParsePlayStatePacket(int packetId, SequenceReader<byte> reader)
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
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not yet implemented", Ip, State, packetId);
                return null;
            default:
                _logger.LogWarning("[{IP}] [{State}] The given packet Id {PacketId} is not valid/implemented in this state", Ip, State, packetId);
                return null;
        }

    }
    
    public void Dispose()
    {
        Tcp.Dispose();
    }
}