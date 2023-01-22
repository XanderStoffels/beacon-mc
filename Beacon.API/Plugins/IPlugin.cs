using Beacon.API.Plugins.Services;

namespace Beacon.API.Plugins;

public interface IPlugin
{
    
    void ConfigureServices(IServiceRegistration services);
    Task EnableAsync();
    Task DisableAsync();
}