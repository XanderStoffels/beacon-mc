using Beacon.API.Plugins;

namespace Beacon.Server.Plugins
{
    internal interface IPluginDiscovery
    {
        List<IBeaconPlugin> DiscoverPlugins(CancellationToken cToken = default);
    }
}