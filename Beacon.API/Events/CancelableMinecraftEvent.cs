namespace Beacon.API.Events;

public abstract class CancelableMinecraftEvent : MinecraftEvent, ICancelable
{
    protected CancelableMinecraftEvent(IServer server) : base(server)
    {
    }

    public bool IsCancelled { get; set; }
}