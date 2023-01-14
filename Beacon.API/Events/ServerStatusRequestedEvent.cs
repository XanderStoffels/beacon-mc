namespace Beacon.API.Events;

public class ServerStatusRequestedEvent : MinecraftEvent, ICancellable
{
    public bool IsCanceled { get; set; }
    public required string IpAddress { get; init; }
    public required ServerStatus ServerStat { get; init; }
}