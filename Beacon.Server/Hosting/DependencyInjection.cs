using Beacon.API.Events;
using Beacon.Plugins;
using Beacon.Server.Plugins;
using Beacon.Server.Plugins.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Beacon.Server.Hosting;

public static class DependencyInjection
{
    public static IHostBuilder UseBeacon(this IHostBuilder builder, Action<ServerEnvironment> optionsAction)
    {
        var options = new ServerEnvironment();
        optionsAction(options);

        builder
            .ConfigureServices(services =>
            {
                services.AddSingleton(options);
                services.AddSingleton(options.ServerConfiguration);
                services.AddSingleton(options.HostingConfiguration);
                services.AddSingleton<IPluginLoader, LocalAssemblyPluginLoader>();
                services.AddSingleton<PluginManager>();
                services.AddSingleton<IMinecraftEventBus>(p => p.GetRequiredService<PluginManager>());
                services.AddSingleton<BeaconServer>();
                services.AddHostedService<BeaconHostingService>();
            });
        return builder;
    }
}