using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beacon.Server.Net.Packets.Handshaking.Serverbound;
using Beacon.Server.Net.Packets.Status.Clientbound;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Net;

public sealed class ClientConnection
{
    private readonly TcpClient _tcp;
    private readonly BeaconServer _server;
    private readonly ILogger _logger;
    private Stream NetworkStream => _tcp.GetStream();

    public EndPoint? RemoteEndPoint => _tcp.Client.RemoteEndPoint;
    public string? IP => RemoteEndPoint?.ToString();
    public ConnectionState State { get; internal set; }
    public bool ExpectLegacyPing { get; set; }
    

    public ClientConnection(TcpClient tcp, BeaconServer server, ILogger logger)
    {
        _tcp = tcp;
        _server = server;
        _logger = logger;
        State = ConnectionState.Handshaking;
        ExpectLegacyPing = true;
    }

    public async Task AcceptPacketsAsync(CancellationToken cancelToken)
    {
        try
        {
            while (!cancelToken.IsCancellationRequested && _tcp.Connected)
            {
                _logger.LogDebug("[{IP}] [{State}] Accepting next packet", IP, State);
                var (packetId, payloadStream) = await ReadPacketIntoMemory(cancelToken);
                _logger.LogDebug("[{IP}] [{State}] {PacketId} with length {PacketLength}",  IP, State, packetId, payloadStream.Length);
                switch (State)
                {
                    case ConnectionState.Handshaking:
                        await HandleHandshakeState(packetId, payloadStream, cancelToken);
                        break;
                    case ConnectionState.Status:
                        await HandleStatusState(packetId, payloadStream, cancelToken);
                        break;
                    case ConnectionState.Login:
                        await HandleLoginState(packetId, payloadStream, cancelToken);
                        break;
                    case ConnectionState.Play:
                        await HandlePlayState(packetId, payloadStream, cancelToken);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                _logger.LogDebug("[{IP}] [{State}] Handled packet {PacketId}", IP, State, packetId);
                await payloadStream.DisposeAsync();
            }
        }
        catch (EndOfStreamException)
        {
            // Can happen while reading exact amount of bytes from the stream. Are you trying to read too much data?
            // Some old Minecraft packets have a different format where they don't send the packet size. Legacy Server list ping for example.
            _logger.LogWarning("[{IP}] [{State}] Unexpected end of stream: expected more data. Closing the connection", IP, State);
            Dispose();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[{IP}] [{State}] Error while accepting packet. Closing the connection", IP, State);
            Dispose();
        }
        
    }
    
    private async Task HandleHandshakeState(int packetId, Stream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Handshake
                var packet = ObjectPool<HandshakePacket>.Shared.Get();
                await packet.ReadPacketAsync(payloadStream);
                await packet.HandleAsync(_server, this);
                ObjectPool<HandshakePacket>.Shared.Return(packet);
                break;

            case 0xFE when ExpectLegacyPing:
                _logger.LogInformation("[{IP}] [{State}] Received unsupported legacy ping packet", IP, State);
                ExpectLegacyPing = false;
                Dispose();
                return;
                
            default:
                await HandleInvalidPacket(packetId);
                return;
        }
    }
    private async Task HandleStatusState(int packetId, MemoryStream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Status Request
                var status = _server.Status;
                var packet = ObjectPool<StatusResponsePacket>.Shared.Get();
                packet.StatusAsJson = JsonSerializer.Serialize(status,  new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                await packet.SerializeAsync(NetworkStream);
                _logger.LogDebug("Send server status success");
                break;
            
            
            case 0xFE when ExpectLegacyPing: // Legacy client server list ping.
                // This can happen in in this state in the event of a malformed server list pong packet that has been sent to the client.
                _logger.LogInformation("[{IP}] [{State}] Received unsupported legacy ping packet", IP, State);
                ExpectLegacyPing = false;
                Dispose();
                return;
            
            case 0x01: // Ping Request
            default:
                ExpectLegacyPing = false;
                await HandleInvalidPacket(packetId);
                return;
        } 
        
    }
    private async Task HandleLoginState(int packetId, MemoryStream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Login Start
            case 0x01: // Encryption Response
            case 0x02: // Login Plugin Response
            default:
                await HandleInvalidPacket(packetId);
                return;
        }    

    }
    private async Task HandlePlayState(int packetId, MemoryStream payloadStream, CancellationToken cancelToken)
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
            default:
                await HandleInvalidPacket(packetId);
                return;
        }

    }
    private ValueTask HandleInvalidPacket(int packetId)
    {
        _logger.LogWarning("[{IP}] Packet ID {PacketId} not handled in {State} state. Closing connection", IP, packetId, State);
        Dispose();
        return ValueTask.CompletedTask;
    }
    private async Task<(int packetId, MemoryStream packetDataStream)> ReadPacketIntoMemory(
        CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
        
        // Get the size of the next packet.
        var packetSize = (await NetworkStream.ReadVarIntAsync()).value;

        if (packetSize == 0xFE && ExpectLegacyPing)
        {
            // Backwards compat with a legacy server ping. This packet is formatted differently.
            _ = await NetworkStream.ReadUnsignedByteAsync(); // Always 0x01
            return (0xFE, MemoryStreaming.Manager.GetStream());
        }
        
        var (packetId, packetHeaderSize) = await NetworkStream.ReadVarIntAsync();
        var payloadSize = packetSize - packetHeaderSize;

        if (payloadSize == 0)
            return (packetId, MemoryStreaming.Manager.GetStream());
        
            
        // The rented array will be _at least_ payloadSize.
        var buffer = ArrayPool<byte>.Shared.Rent(payloadSize);
        try
        {
            // We only want to use the first payloadSize bytes.
            var bufferAsMemory = buffer.AsMemory(0, payloadSize);

            // Load the data in memory.
            await NetworkStream.ReadExactlyAsync(bufferAsMemory, cancelToken);

            // We wrap the buffer in a recyclable memory stream, making it easier to handle.
            // Using a separate memory stream for this so we can first load all the data in memory at once for performance.
            var payloadMemoryStream = MemoryStreaming.Manager.GetStream(buffer);
            return (packetId, payloadMemoryStream);
        }
        finally
        {
            // We can return the buffer to the pool now. It will no longer be used by the memory stream.
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }


    public void Dispose()
    {
        _tcp.Dispose();
    }
}