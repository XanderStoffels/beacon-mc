using Beacon.API.Events;
using Beacon.Plugins;
using Beacon.Server.Plugins;
using Beacon.Server.Plugins.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beacon.Server.Hosting;

public static class DependencyInjection
{
    public static IHostBuilder UseBeacon(this IHostBuilder builder, Action<BeaconHostingOptions> optionsAction)
    {
        var options = new BeaconHostingOptions();
        optionsAction(options);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(options.ServerOptions);
            services.AddSingleton<LocalAssemblyPluginLoader>();
            services.AddSingleton<IPluginLoader, LocalAssemblyPluginLoader>();
            services.AddSingleton<PluginManager>();
            services.AddSingleton<IMinecraftEventBus>(p => p.GetRequiredService<PluginManager>());
            services.AddHostedService<BeaconServer>();
        });
        return builder;
    }
}