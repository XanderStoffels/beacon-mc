using Beacon.API.Plugins.Services;

namespace Beacon.API.Plugins;

public interface IPlugin
{
    /// <summary>
    /// Configure any services that this plugin provides.
    /// Services include Event Handlers, Commands, shared services and more.
    /// </summary>
    /// <remarks>Gets called when the plugin is loaded, before <see cref="EnableAsync"/></remarks>
    /// <param name="services"></param>
    void ConfigureServices(IServiceRegistration services);
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task EnableAsync();
    Task DisableAsync();
}