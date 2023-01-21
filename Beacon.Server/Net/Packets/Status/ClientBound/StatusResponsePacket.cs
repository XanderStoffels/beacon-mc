using System.Text.Json;
using System.Text.Json.Serialization;
using Beacon.API;
using Beacon.Server.Utils;
using Beacon.Server.Utils.Extensions;

namespace Beacon.Server.Net.Packets.Status.ClientBound;

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

        var idLength = PacketId.GetVarIntLength();
        var json = StatusAsJson;
        var jsonLength = json.GetVarStringLength();
        var packetLength = idLength + jsonLength;

        await stream.WriteVarIntAsync(packetLength);
        await stream.WriteVarIntAsync(PacketId);
        await stream.WriteStringAsync(json);
        await stream.FlushAsync();
    }
    
    private string StatusAsJson => JsonSerializer.Serialize(ServerStatus,
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    
}