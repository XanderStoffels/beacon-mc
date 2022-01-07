using Beacon.API.Plugins;

namespace Beacon.Server.Plugins
{
    internal interface IPluginDiscoverer
    {
        List<IBeaconPlugin> DiscoverPlugins(CancellationToken cToken = default);
    }
}