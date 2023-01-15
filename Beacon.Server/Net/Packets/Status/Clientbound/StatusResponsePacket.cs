using System.Text.Json;
using System.Text.Json.Serialization;
using Beacon.API;
using Beacon.Server.Utils;

namespace Beacon.Server.Net.Packets.Status.Clientbound;

/// <summary>
/// A response to the client's status request.
/// </summary>
public class StatusResponsePacket
{
    public const int PacketId = 0x00;
    public ServerStatus? ServerStatus { get; set; }

    public async Task SerializeAsync(Stream stream)
    {
        if (ServerStatus == null)
            throw new ArgumentNullException(nameof(ServerStatus));
        
        using var memory = MemoryStreaming.Manager.GetStream();
        await memory.WriteVarIntAsync(PacketId);
        await memory.WriteStringAsync(StatusAsJson);
        
        await stream.WriteVarIntAsync((int)memory.Length);
        memory.WriteTo(stream);
        await stream.FlushAsync();
    }
    
    private string StatusAsJson => JsonSerializer.Serialize(ServerStatus,
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
}