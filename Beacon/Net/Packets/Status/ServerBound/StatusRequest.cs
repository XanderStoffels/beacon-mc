using System.Buffers;
using Beacon.Net.Packets.Status.ClientBound;

namespace Beacon.Net.Packets.Status.ServerBound;

/// <summary>
/// A packet sent from the client to the server to request the server's status.
/// </summary>
/// <remarks>This class is a Singleton. This packet has no fields, so no deserialization is needed.</remarks>
public sealed class StatusRequest : IServerBoundPacket
{
    public const int PacketId = 0x00;
    
    // Singleton
    public static StatusRequest Instance => _instance ??= new StatusRequest();
    private static StatusRequest? _instance;
    private StatusRequest() {}
    
    public void Handle(Server server, Connection connection)
    {
        var res = StatusResponse.Rent();
        connection.EnqueuePacket(res);
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        throw new NotImplementedException();
    }
}
