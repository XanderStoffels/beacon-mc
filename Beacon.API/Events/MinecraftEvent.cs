namespace Beacon.API.Events;

public abstract class MinecraftEvent
{
    public IServer Server { get; }

    protected MinecraftEvent(IServer server)
    {
        Server = server;
    }
}
