using Beacon.API.Plugins;
using Beacon.PluginEngine;

namespace Beacon.Server.Plugins.Local
{
    public class PluginAssemblyContainer : IPluginContainer
    {
        public IBeaconPlugin? Plugin { get; private set; }
        public PluginAssemblyLoadContext LoadContext { get; }

        public string Name { get; }

        public PluginAssemblyContainer(IBeaconPlugin plugin, PluginAssemblyLoadContext loadContext)
        {
            Plugin = plugin;
            Name = plugin.Name;
            LoadContext = loadContext;
        }

        public ValueTask UnloadAsync()
        {
            Plugin = null;
            LoadContext.Unload();
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return UnloadAsync();
        }
    }
}
