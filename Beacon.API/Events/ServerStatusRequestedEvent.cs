using Beacon.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API.Events
{
    public class ServerStatusRequestedEvent : MinecraftEvent
    {
        public EndPoint? Endpoint { get; }
        public ServerStatus ServerStatus { get; }

        public ServerStatusRequestedEvent(EndPoint? endpoint, ServerStatus serverStatus)
        {
            Endpoint = endpoint;
            ServerStatus = serverStatus;
        }
    }
}
