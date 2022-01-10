using Beacon.API.Plugins;

namespace Beacon.Server.Plugins
{
    internal interface IPluginLoader
    {
        ValueTask<List<IPluginContext>> LoadPluginContexts(CancellationToken cToken = default);
    }
}