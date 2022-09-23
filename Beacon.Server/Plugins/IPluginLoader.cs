using Beacon.Server.Plugins;

namespace Beacon.Plugins;

internal interface IPluginLoader
{
    public Task<List<PluginContainer>> LoadAsync(CancellationToken cToken = default);
}