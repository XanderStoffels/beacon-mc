namespace Beacon.API.Events
{
    public interface IMinecraftEventBus
    {
        public Task FireEventAsync<TEvent>(TEvent e, CancellationToken cancelToken = default) where TEvent : MinecraftEvent;

    }
}
