using Beacon.Util;

namespace Beacon.Net.Packets.Configuration.ClientBound;

public class SelectKnownPacksFromServer : Rentable<SelectKnownPacksFromServer>, IClientBoundPacket
{
    public const int PacketId = 0x0E;

    public List<KnownPack> Packs { get; set; } = [];
    
    public bool SerializePayload(Span<byte> buffer, out int bytesWritten)
    {
        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteVarInt(Packs.Count);
        
        foreach (var pack in Packs)
        {
            writer.WriteVarString(pack.Namespace);
            writer.WriteVarString(pack.Id);
            writer.WriteVarString(pack.Version);
        }
        
        return writer.IsSuccess(out bytesWritten);
    }
}