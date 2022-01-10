using Beacon.API.Plugins;
using System.Runtime.Loader;

namespace Beacon.Server.Plugins
{
    internal class PluginContext : IPluginContext
    {
        public IBeaconPlugin Plugin { get; private set; }
        public AssemblyLoadContext AssemblyContext { get; private set; }

        public PluginContext(IBeaconPlugin plugin, AssemblyLoadContext assemblyContext)
        {
            Plugin = plugin;
            AssemblyContext = assemblyContext;
        }

        public async ValueTask DisposeAsync()
        {
            await Plugin.Disable();
            Plugin = null;
            AssemblyContext.Unload();
        }
    }
}
