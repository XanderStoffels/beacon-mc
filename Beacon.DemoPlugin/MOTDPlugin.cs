using Beacon.API;
using Beacon.API.Events;
using Beacon.API.Plugins;
using Beacon.DemoPlugin.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.DemoPlugin
{
    public class MOTDPlugin : IBeaconPlugin
    {
        public string Name => "Demo Plugin";

        public Version Version => new(0, 1);

        public ValueTask Enable()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Disable()
        {
            return ValueTask.CompletedTask;
        }

        
        public void RegisterServices(IServiceCollection services)
        {
            services.AddEventHandler<ServerStatusRequestedEvent, StatusPrinter>();
        }
    }
}
