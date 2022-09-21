using Beacon.API;
using Beacon.API.Events;
using Beacon.API.Plugins;
using Beacon.API.Plugins.Services;
using Beacon.DemoPlugin.Commands;
using Beacon.DemoPlugin.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.DemoPlugin;

public class MOTDPlugin : IBeaconPlugin
{
    public string Name => "Demo Plugin";
    public Version Version => new(0, 1);


    public void ConfigureServices(IServiceRegistrator registrator)
    {
        // 
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
