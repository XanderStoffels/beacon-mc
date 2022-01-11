
using Beacon.API;
using Beacon.API.Events;
using Beacon.API.Events.Handling;

namespace Beacon.Server.Plugins
{
    internal interface IPluginController
    {
        bool IsInitialized { get; }
        ValueTask LoadAsync(IServer server);
        ValueTask ReloadAsync(IServer server);
        ValueTask UnloadAsync();

        IReadOnlyList<IMinecraftEventHandler<TEvent>> GetPluginEventHandlers<TEvent>() where TEvent : MinecraftEvent;
    }
}