namespace Beacon.API.Events.Handling;

public interface IMinecraftEventHandler<TEvent> where TEvent : MinecraftEvent
{
    public Priority Priority { get; }
    public Task HandleAsync(TEvent e, CancellationToken cancelToken);
}