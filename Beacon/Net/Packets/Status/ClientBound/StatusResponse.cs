using System.Text.Json;
using System.Text.Json.Serialization;
using Beacon.Util;

namespace Beacon.Net.Packets.Status.ClientBound;

public sealed class StatusResponse : IClientBoundPacket
{
    private const int PacketId = 0x00;
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public Connection Connection { get; private set; }
    public StatusResponse(Connection connection)
    {
        Connection = connection;
    }

    public bool TryWritePayloadTo(Span<byte> buffer, out int bytesWritten)
    {
        bytesWritten = 0;
        
        var serverStatus = new ServerStatus(
            new Version("1.21.5", 770),
            new Players(100, 1, [new Sample("Notch", Guid.Empty.ToString())]),
            // Powered by Beacon, beacon in cyan color.
            new Description("§fPowered by §6Beacon§f!"),
            //Icons.Beacon, 
            string.Empty,
            false
        );
        
        var jsonResponse = JsonSerializer.Serialize(serverStatus, JsonOptions);

        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteString(jsonResponse);
        return writer.IsSuccess(out bytesWritten);
    }
    
}