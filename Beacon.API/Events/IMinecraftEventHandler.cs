namespace Beacon.API.Events;

public interface IMinecraftEventHandler<TEvent>
    where TEvent : MinecraftEvent
{
    public Task Handle(TEvent e, CancellationToken cancelToken);
}