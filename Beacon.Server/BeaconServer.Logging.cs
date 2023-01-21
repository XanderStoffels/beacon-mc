using Beacon.Server.Net;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public sealed partial class BeaconServer
{
    private readonly ILogger _logger;
    public ILogger Logger => _logger;
    
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "[{IP}] [{State}] Handled packet with ID {PacketId} after {Ms}ms")] 
    private partial void LogPacketHandled(string? ip, ConnectionState state, int packetId, int ms);
    
    

}
