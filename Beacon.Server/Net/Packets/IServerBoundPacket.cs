namespace Beacon.Server.Net.Packets;

public interface IServerBoundPacket
{
    ValueTask HandleAsync(BeaconServer server, ClientConnection client);
}