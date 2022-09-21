using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Microsoft.Extensions.Logging;

namespace Beacon.DemoPlugin.Handlers;

internal class LocalHostBlocker : MinecraftEventHandler<TcpConnectedEvent>
{
    public override Task HandleAsync(TcpConnectedEvent e, CancellationToken cancelToken)
    {
        if (e.Connection.RemoteAddress.StartsWith("127.0.0.1")) {
            e.IsCancelled = true;
        }
        return Task.CompletedTask;
    }
}
