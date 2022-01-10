
using Beacon.API.Events;
using Beacon.API.Events.Handling;

namespace Beacon.Server.Plugins
{
    internal interface IPluginController
    {
        bool IsInitialized { get; }
        ValueTask LoadAsync();
        IReadOnlyList<IMinecraftEventHandler<TEvent>> GetPluginEventHandlers<TEvent>() where TEvent : MinecraftEvent;
        ValueTask ReloadAsync();
        ValueTask UnloadAll();
    }
}