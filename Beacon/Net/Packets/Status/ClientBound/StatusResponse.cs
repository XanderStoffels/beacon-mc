using System.Text.Json;
using System.Text.Json.Serialization;
using Beacon.Api;
using Beacon.Util;

namespace Beacon.Net.Packets.Status.ClientBound;

public sealed class StatusResponse : Rentable<StatusResponse>, IClientBoundPacket
{
    private const int PacketId = 0x00;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool SerializePayload(Span<byte> buffer, out int bytesWritten)
    {
        // TODO: Dummy implementation. The server status should be set by the StatusRequest Handle() method before enqueuing this packet.
        bytesWritten = 0;
        
        var serverStatus = new ServerStatus(
            "1.21.5",
            770,
            100,
            1,
            "§fPowered by §6Beacon§f!",
            string.Empty,
            false,
            new ServerStatus.StatusPlayerSample("§6Notch", "069a79f4-44e9-4726-a5be-fca90e38aaf5")
        );
        
        var jsonResponse = JsonSerializer.Serialize(serverStatus, JsonOptions);

        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteString(jsonResponse);
        return writer.IsSuccess(out bytesWritten);
    }
}