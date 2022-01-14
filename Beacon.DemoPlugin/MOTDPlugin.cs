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

        public ValueTask EnableAsync()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask DisableAsync()
        {
            return ValueTask.CompletedTask;
        }

        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEventHandler<ServerStatusRequestEvent, StatusPrinter>();
            services.AddEventHandler<TcpConnectedEvent, LocalHostBlocker>();

        }
    }
}
