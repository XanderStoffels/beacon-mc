using Beacon.API;

namespace Beacon.Server.Net.Packets.Status.Clientbound;

/// <summary>
/// A response to the client's status request.
/// </summary>
public class StatusResponsePacket
{
    public const int PacketId = 0x00;
    public string StatusAsJson { get; set; } = string.Empty;

    public void Serialize(Stream stream, bool flush = true)
    {
         stream.WriteVarInt(PacketId);
         stream.WriteString(StatusAsJson);
         if (flush) stream.Flush();
    }

    public async Task SerializeAsync(Stream stream, bool flush = true)
    {
         await stream.WriteVarIntAsync(PacketId);
         await stream.WriteStringAsync(StatusAsJson);
         if (flush) await stream.FlushAsync();
    }

}