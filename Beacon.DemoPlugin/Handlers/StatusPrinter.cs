using Beacon.API.Events;
using Beacon.API.Events.Handling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.DemoPlugin.Handlers
{
    internal class StatusPrinter : MinecraftEventHandler<ServerStatusRequestedEvent>
    {
        public override ValueTask HandleAsync(ServerStatusRequestedEvent e, CancellationToken cancelToken)
        {
            e.ServerStatus.Description.Text = "Message altered by a plugin";
            return ValueTask.CompletedTask;
        }
    }
}
