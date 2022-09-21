using Beacon.API.Events;
using Beacon.API.Events.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.DemoPlugin.Handlers
{
    internal class StatusPrinter : MinecraftEventHandler<ServerStatusRequestEvent>
    {
        public override Task HandleAsync(ServerStatusRequestEvent e, CancellationToken cancelToken)
        {
            e.ServerStatus.Description.Text = "Message altered by a plugin";
            return Task.CompletedTask;
        }
    }
}
