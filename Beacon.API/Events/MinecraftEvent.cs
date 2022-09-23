namespace Beacon.API.Events;

public abstract class MinecraftEvent
{
    protected MinecraftEvent(IServer server)
    {
        Server = server;
    }

    public IServer Server { get; }
}