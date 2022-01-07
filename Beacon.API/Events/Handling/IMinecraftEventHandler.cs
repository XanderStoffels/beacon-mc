namespace Beacon.API.Events.Handling
{
    public interface IMinecraftEventHandler<TEvent> where TEvent : MinecraftEvent 
    {
        public ValueTask HandleAsync(TEvent e, CancellationToken cancelToken);
    }
}
