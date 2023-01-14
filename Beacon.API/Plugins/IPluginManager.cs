namespace Beacon.API.Plugins;

public interface IPluginManager
{
    Task FireEventAsync(CancellationToken cancelToken);
}