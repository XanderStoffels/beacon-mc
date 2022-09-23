using Beacon.API.Plugins.Services;

namespace Beacon.API.Plugins;

public interface IBeaconPlugin
{
    public string Name { get; }
    public Version Version { get; }
    public void ConfigureServices(IServiceRegistrator registrator);
    public Task EnableAsync();
    public Task DisableAsync();
}