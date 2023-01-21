namespace Beacon.Server.Net.Packets;

public record QueuedServerBoundPacket(ClientConnection Connection, IServerBoundPacket Packet, DateTime QueuedAt) : IDisposable
{
    
    public void Dispose()
    {
        Packet.Return();
    }
}