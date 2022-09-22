using Beacon.API.Net;

namespace Beacon.API.Events;

public class TcpDisconnectedEvent : MinecraftEvent
{
    public bool HadError { get; }
    public string RemoteAddress { get; }

    public TcpDisconnectedEvent(IServer server, bool hadError, string remoteAddress) : base(server)
    {
        HadError = hadError;
        RemoteAddress = remoteAddress;
    }
}
