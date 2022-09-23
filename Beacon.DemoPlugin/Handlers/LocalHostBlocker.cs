using Beacon.API.Events;
using Beacon.API.Events.Handling;

namespace Beacon.DemoPlugin.Handlers;

internal class LocalHostBlocker : IMinecraftEventHandler<TcpConnectedEvent>
{
    public Priority Priority => Priority.NORMAL;

    public Task HandleAsync(TcpConnectedEvent e, CancellationToken cancelToken)
    {
        e.IsCancelled = e.Connection.RemoteAddress.StartsWith("127.0.0.1");
        return Task.CompletedTask;
    }
}