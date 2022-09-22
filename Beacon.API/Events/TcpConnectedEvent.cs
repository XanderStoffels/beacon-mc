using Beacon.API.Net;

namespace Beacon.API.Events;

public class TcpConnectedEvent : CancelableMinecraftEvent
{
    public IConnection Connection { get; }

    public TcpConnectedEvent(IServer server, IConnection connection) : base(server)
    {
        Connection = connection;
    }
}
