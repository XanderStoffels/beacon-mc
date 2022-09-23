using Beacon.API.Models;

namespace Beacon.API.Events;

public class ServerStatusRequestEvent : CancelableMinecraftEvent
{
    public ServerStatusRequestEvent(IServer server, string endpoint, ServerStatus serverStatus) : base(server)
    {
        Endpoint = endpoint;
        ServerStatus = serverStatus;
    }

    public string Endpoint { get; }
    public ServerStatus ServerStatus { get; }
}