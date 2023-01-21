namespace Beacon.Server.Net.Packets;

public record QueuedServerboundPacket(ClientConnection Connection, IServerBoundPacket Packet) : IDisposable
{
    public void Dispose()
    {
        Packet.Return();
    }
}