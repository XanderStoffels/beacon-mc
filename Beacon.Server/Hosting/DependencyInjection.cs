using Beacon.API.Events;
using Beacon.Server.Plugins;
using Beacon.Server.Plugins.Local;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Server.Hosting;
public static class DependencyInjection
{
    public static IServiceCollection AddBeacon(this IServiceCollection services) => AddBeacon(services, _ => { });

    public static IServiceCollection AddBeacon(this IServiceCollection services, Action<ServerOptions> configureOptions)
    {
        var options = new ServerOptions();
        configureOptions(options);
        
        services.AddSingleton(options);
        services.AddSingleton<LocalAssemblyPluginLoader>();
        services.AddSingleton<PluginManager>();
        services.AddSingleton<IMinecraftEventBus>(p => p.GetRequiredService<PluginManager>());
        services.AddHostedService<BeaconServer>();
        return services;
    }
}
