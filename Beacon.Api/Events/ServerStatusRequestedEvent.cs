using System.Net;

namespace Beacon.Api.Events;

public class ServerStatusRequestedEvent 
{
    public IPEndPoint RequestedBy { get; }
}

public interface ICancellable
{
    /// <summary>
    /// Indicates whether the event has been cancelled.
    /// </summary>
    bool IsCancelled { get; set; }
}