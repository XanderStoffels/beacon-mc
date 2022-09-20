using Beacon.API;
using Beacon.API.Events;
using Beacon.API.Plugins;
using Beacon.DemoPlugin.Commands;
using Beacon.DemoPlugin.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.DemoPlugin;

public class MOTDPlugin : IBeaconPlugin
{
    public string Name => "Demo Plugin";
    public Version Version => new(0, 1);

    public ValueTask EnableAsync() => ValueTask.CompletedTask;
    public ValueTask DisableAsync() => ValueTask.CompletedTask;

    public void ConfigureServices(IServiceCollection services) =>
         services
            // Commands
            .AddCommand<HelloCommand>()
            .AddCommand<ClearCommand>()

            // Events
            .AddEventHandler<ServerStatusRequestEvent, StatusPrinter>()
            .AddEventHandler<TcpConnectedEvent, LocalHostBlocker>();
}
