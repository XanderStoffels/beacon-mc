
using Beacon.API.Events;
using Beacon.API.Events.Handling;

namespace Beacon.Server.Plugins
{
    internal interface IPluginController
    {
        bool IsInitialized { get; }
        ValueTask InitializePlugins();
        List<IMinecraftEventHandler<TEvent>> GetEventHandlers<TEvent>() where TEvent : MinecraftEvent;
        ValueTask<TEvent> FireEventAsync<TEvent>(TEvent e, CancellationToken cToken = default) where TEvent : MinecraftEvent;
    }
}