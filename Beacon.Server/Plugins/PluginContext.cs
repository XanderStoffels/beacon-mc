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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Plugin = null;     // PluginContext should no longer be used after disposing.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            AssemblyContext.Unload();
        }
    }
}
