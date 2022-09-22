using Beacon.API.Models;

namespace Beacon.API.Events
{
    public class ServerStatusRequestEvent : CancelableMinecraftEvent
    {
        public string Endpoint { get; }
        public ServerStatus ServerStatus { get; }
        public ServerStatusRequestEvent(IServer server, string endpoint, ServerStatus serverStatus) : base(server)
        {
            Endpoint = endpoint;
            ServerStatus = serverStatus;
        }
    }
}
