namespace Beacon.API.Events;

public interface IMinecraftEventHandler<in TEvent>
    where TEvent : MinecraftEvent
{
    public Priority Priority { get; } 
    public Task HandleAsync(TEvent e, CancellationToken cancelToken);
}