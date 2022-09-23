using Beacon.API.Events;
using Beacon.API.Plugins;
using Beacon.API.Plugins.Services;
using Beacon.DemoPlugin.Commands;
using Beacon.DemoPlugin.Handlers;

namespace Beacon.DemoPlugin;

public class MOTDPlugin : IBeaconPlugin
{
    public string Name => "Demo Plugin";
    public Version Version => new(0, 1);


    public void ConfigureServices(IServiceRegistrator registrator)
    {
        registrator.RegisterEventHandler<ServerStatusRequestEvent, StatusPrinter>();
        registrator.RegisterCommand<ClearCommand>();
    }

    public Task EnableAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisableAsync()
    {
        return Task.CompletedTask;
    }
}