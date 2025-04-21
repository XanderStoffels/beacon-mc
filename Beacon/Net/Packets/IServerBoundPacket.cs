using System.Buffers;

namespace Beacon.Net.Packets;

public interface IServerBoundPacket
{
    /// <summary>
    /// Handle the incoming packet.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="connection">The connection that sent this packet to the server.</param>
    public void Handle(Server server, Connection connection);
    
    /// <summary>
    /// Hydrate the packet from the given reader.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns>Indication if the hydration was successful. If not, the state of the packet is unchanged.</returns>
    public bool DeserializePayload(ref SequenceReader<byte> reader);

}