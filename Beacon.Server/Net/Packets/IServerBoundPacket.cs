using System.Buffers;

namespace Beacon.Server.Net.Packets;

public interface IServerBoundPacket
{
    public int Id { get; }
    ValueTask HandleAsync(BeaconServer server, ClientConnection client);

    public static bool TryRentAndFill(SequenceReader<byte> reader, out IServerBoundPacket? packet)
    {
        throw new InvalidOperationException();
    }
    public void Return();
}