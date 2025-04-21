using System.Buffers;

namespace Beacon.Net.Packets.Configuration.ServerBound;

/// <summary>
/// Sent by the client to notify the server that the configuration process has finished. It is sent in response to the server's Finish Configuration.
/// </summary>
/// <remarks>
/// This class has no logic; the connection should itself handle the packet and switch to Play state.
/// </remarks>
public sealed class AckFinishConfiguration : IServerBoundPacket
{
    public const int PacketId = 0x03;
    public void Handle(Server server, Connection connection)
    {
        // Nothing to do here.
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        return true;
    }
}