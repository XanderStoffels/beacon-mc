using Beacon.API.Events;

namespace Beacon.Server.Plugins.Events
{
    internal interface IMinecraftEventBus
    {
        public ValueTask<TEvent> FireEventAsync<TEvent>(TEvent e, CancellationToken cToken = default) where TEvent : MinecraftEvent;
    }
}
