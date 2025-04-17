namespace Beacon.Net.Packets.Status;

public readonly record struct QueuedClientBoundPacket(IClientBoundPacket Packet, Connection Target);
public readonly record struct QueuedServerBoundPacket(IServerBoundPacket Packet, Connection Origin);