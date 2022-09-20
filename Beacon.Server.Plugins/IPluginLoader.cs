using Beacon.API.Plugins;

namespace Beacon.Plugins;

public interface IPluginLoader
{
    ValueTask<List<IPluginContainer>> LoadAsync(CancellationToken cToken = default);
}
