namespace Beacon.Server.Net.Packets;

public record QueuedServerboundPacket(ClientConnection Connection, IServerBoundPacket Packet);