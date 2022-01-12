using Beacon.API.Plugins;
using Beacon.PluginEngine;

namespace Beacon.Server.Plugins
{
    public interface IPluginLoader
    {
        ValueTask<List<IPluginContainer>> LoadAsync(CancellationToken cToken = default);
    }
}
