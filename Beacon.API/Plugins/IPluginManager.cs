using Beacon.API.Events;

namespace Beacon.API.Plugins;

public interface IPluginManager
{
    TService? GetService<TService>();
    Task FireEventAsync<TEvent>(TEvent e, CancellationToken cancelToken)
        where TEvent : MinecraftEvent;
}