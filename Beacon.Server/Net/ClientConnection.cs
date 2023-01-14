using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.IO;

namespace Beacon.Server.Net;

public sealed class ClientConnection
{
    private readonly TcpClient _tcp;
    private readonly BeaconServer _server;
    private readonly ILogger _logger;
    private Stream NetworkStream => _tcp.GetStream();

    public ConnectionState State { get; private set; }
    public EndPoint? RemoteEndPoint => _tcp.Client.RemoteEndPoint;

    public ClientConnection(TcpClient tcp, BeaconServer server, ILogger logger)
    {
        _tcp = tcp;
        _server = server;
        State = ConnectionState.Handshaking;
    }

    public async Task AcceptPacketsAsync(CancellationToken cancelToken)
    {
        
        while (!cancelToken.IsCancellationRequested && _tcp.Connected)
        {
            var (nextId, payloadStream) = await ReadPacketIntoMemory(cancelToken);
            switch (State)
            {
                case ConnectionState.Handshaking:
                    await HandleHandshakeState(nextId, payloadStream, cancelToken);
                    break;
                case ConnectionState.Status:
                    await HandleStatusState(nextId, payloadStream, cancelToken);
                    break;
                case ConnectionState.Login:
                    await HandleLoginState(nextId, payloadStream, cancelToken);
                    break;
                case ConnectionState.Play:
                    await HandlePlayState(nextId, payloadStream, cancelToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await payloadStream.DisposeAsync();
        }
    }
    

    public async Task HandleHandshakeState(int packetId, Stream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Handshake
                break;
            
            case 0xFE: // Legacy client server list ping
                break;
            
            default:
                await HandleInvalidPacket(packetId);
                break;
        }
    }
    private async Task HandleStatusState(int packetId, MemoryStream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Status Request
                break;
            
            case 0x01: // Ping Request
                break;
            
            default:
                await HandleInvalidPacket(packetId);
                break;
        }    
    }
    private async Task HandleLoginState(int packetId, MemoryStream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Login Start
                break;
            
            case 0x01: // Encryption Response
                break;
            
            case 0x02: // Login Plugin Response
                break;
            
            default:
                await HandleInvalidPacket(packetId);
                break;
        }    
    }
    private async Task HandlePlayState(int packetId, MemoryStream payloadStream, CancellationToken cancelToken)
    {
        switch (packetId)
        {
            case 0x00: // Confirm Teleportation
                return;
            
            case 0x01: // Query Block Entity Tag
                return;
            
            case 0x02: // Change Difficulty
                return;
            
            case 0x03: // Message Acknowledgment
                return;
            
            case 0x04: // Chat Command
                return;
            
            case 0x05: // Chat Message
                return;
            
            case 0x06: // Client Command
                return;
            
            case 0x07: // Client Information
                return;
            
            case 0x08: // Command Suggestion Request
                return;
            
            case 0x09: // Click Container Button
                return;
            
            case 0x0A: //  Click Container
                return;
            
            case 0x0B: //  Close Container
                return;
            
            case 0x0C: //  Plugin Message
                return;
            
            case 0x0D: //  Edit Book
                return;
            
            case 0x0E: // Query Entity Tag
                return;
            
            case 0X0F: //  Interact Entity
                return;
            
            case 0x10: // Jigsaw Generate
                return;
            
            case 0x11: // Keep Alive
                return;
            
            case 0x12: // Lock Difficulty
                return;
            
            case 0x13: // Set Player Position
                return;
            
            case 0x14: // Set Player Position And Rotation
                return;
            
            case 0x015: // Set Player Rotation
                return;
            
            case 0x16: // Set On Ground
                return;
            
            case 0x17: // Move Vehicle
                return;
            
            case 0x18: // Paddle Boat.
                return;
            
            case 0x19: // Pick Item
                return;
            
            case 0x1A: // Place Recipe
                return;
            
            case 0x1B: // Player Abilities
                return;
            
            case 0x1C: // Player Action
                return;
            
            case 0x1D: // Player Command
                return;
            
            case 0x1E: // Player Input
                return;
            
            case 0x1F: // Pong (play)
                return;
            
            case 0x20: // Player Session
                return;
            
            case 0x21: // Change Recipe Book Settings
                return;
            
            case 0x22: // Set Seen Recipe
                return;
            
            case 0x23: // Rename Item
                return;
            
            case 0x24: // Resource Pack
                return;
            
            case 0x25: // Seen Advancements
                return;
            
            case 0x26: // Select Trade
                return;
            
            case 0x27: // Set Beacon Effect
                return;
            
            case 0x28: // Set Held Item
                return;
            
            case 0x29: // Program Command Block
                return;
            
            case 0x2A: // Program Command Block Minecart
                return;
            
            case 0x2B: // Set Create Mode Slot
                return;
            
            case 0x2C: // Program Jigsaw Block
                return;
            
            case 0x2D: // Program Structure Block
                return;
            
            case 0x2E: // Update Sign
                return;
            
            case 0x2F: // Swing Arm
                return;
            
            case 0x30: // Teleport To Entity
                return;
            
            case 0x31: // Use Item On
                return;
            
            case 0x32: // Use Item
                return;
            
            default:
                await HandleInvalidPacket(packetId);
                return;
        }
    }

    private ValueTask HandleInvalidPacket(int packetId)
    {
        _logger.LogWarning("Received unknown packet ID {PacketId} in {State} state", packetId, State);
        _logger.LogWarning("Closing connection");
        Dispose();
        return ValueTask.CompletedTask;
    }

    
    private async Task<(int packetId, MemoryStream packetDataStream)> ReadPacketIntoMemory(
        CancellationToken cancelToken)
    {
        cancelToken.ThrowIfCancellationRequested();
        
        // Get the size of the next packet.
        var (_, packetSize) = await NetworkStream.ReadVarIntAsync();
        var (packetHeaderSize, packetId) = await NetworkStream.ReadVarIntAsync();
        var payloadSize = packetSize - packetHeaderSize;
            
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