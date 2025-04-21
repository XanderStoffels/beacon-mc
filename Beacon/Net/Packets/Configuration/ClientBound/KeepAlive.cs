using Beacon.Util;

namespace Beacon.Net.Packets.Configuration.ClientBound;

public sealed class KeepAlive : Rentable<KeepAlive>, IClientBoundPacket
{
    public const int PacketId = 0x04;
    public long KeepAliveId { get; set; }
    
    public bool SerializePayload(Span<byte> buffer, out int bytesWritten)
    {
        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteLong(KeepAliveId);
        return writer.IsSuccess(out bytesWritten);
    }
}