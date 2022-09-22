using Beacon.API.Events;
using Beacon.API.Events.Handling;

namespace Beacon.DemoPlugin.Handlers
{
    internal class StatusPrinter : IMinecraftEventHandler<ServerStatusRequestEvent>
    {
        public Priority Priority => Priority.NORMAL;

        public Task HandleAsync(ServerStatusRequestEvent e, CancellationToken cancelToken)
        {
            e.ServerStatus.Description.Text = "Message altered by a plugin";
            return Task.CompletedTask;
        }
    }
}
