namespace Beacon.API.Events;

public abstract class CancelableMinecraftEvent : MinecraftEvent, ICancelable
{
    public bool IsCancelled { get; set; }
    protected CancelableMinecraftEvent(IServer server) : base(server)
    {
    }
}
