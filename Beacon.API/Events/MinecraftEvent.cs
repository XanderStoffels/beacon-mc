namespace Beacon.API.Events;

public abstract class MinecraftEvent
{   
    public required IServer Server { get; init; }
}