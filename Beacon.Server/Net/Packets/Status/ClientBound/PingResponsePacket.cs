using Beacon.Server.Utils;
using Beacon.Server.Utils.Extensions;

namespace Beacon.Server.Net.Packets.Status.ClientBound;

public class PingResponsePacket : IClientBoundPacket
{
    private const int PacketId = 0x01;

    public int Id => PacketId;
    public long Payload { get; set; }


    public async Task SerializeAsync(Stream stream)
    {
        const int packetLength = 9; 
        await stream.WriteVarIntAsync(packetLength);
        await stream.WriteVarIntAsync(PacketId); 
        await stream.WriteLongAsync(Payload);
        await stream.FlushAsync();
    }
}