using System.Buffers;
using Beacon.Util;

namespace Beacon.Net.Packets.Configuration.ServerBound;

public class SelectKnownPacksFromClient : Rentable<SelectKnownPacksFromClient>, IServerBoundPacket  
{
    
    public const int PacketId = 0x07;

    public List<KnownPack> Packs { get; set; } = [];
    
    public void Handle(Server server, Connection connection)
    {
        // This packet is sent by the client to inform the server of the packs it has.
        // It has no effect on the game state.
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        Packs.Clear();
        if (!reader.TryReadVarInt(out var amountOfPacks)) return false;
        for (var i = 0; i < amountOfPacks; i++)
        {
            if (!reader.TryReadVarString(out var @namespace)) return false;
            if (!reader.TryReadVarString(out var id)) return false;
            if (!reader.TryReadVarString(out var version)) return false;
            Packs.Add(new KnownPack
            {
                Namespace = @namespace,
                Id = id,
                Version = version
            });
        }

        return true;
    }
}