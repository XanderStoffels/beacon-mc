using Microsoft.Extensions.Logging;

namespace Beacon.Server.Net;

public sealed partial class ClientConnection
{
    private readonly ILogger _logger;
    
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "[{IP}] [{State}] Pipe read was canceled from writer")] 
    private partial void LogPipeCanceledFromWriter(string? ip, ConnectionState state);
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "[{IP}] [{State}] Start accepting packets")] 
    private partial void LogStartAcceptingPackets(string? ip, ConnectionState state);
    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "[{IP}] [{State}] Stopped accepting packets")] 
    private partial void LogStopAcceptingPackets(string? ip, ConnectionState state);
    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "[{IP}] [{State}] Packet with ID {PacketId} queued")] 
    private partial void LogPacketQueued(string? ip, ConnectionState state, int packetId);
    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "[{IP}] [{State}] Unable to write parsed packet (id: {PacketId}) to packet channel")] 
    private partial void LogCouldNotQueuePacket(string? ip, ConnectionState state, int packetId);
    [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "[{IP}] [{State}] Received unsupported legacy ping packet")] 
    private partial void LogUnsupportedLegacyPing(string? ip, ConnectionState state);
    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "[{IP}] [{State}] Packet Id {PacketId} is not valid/implemented in this state")] 
    private partial void LogInvalidPacket(string? ip, ConnectionState state, int packetId);
    [LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "[{IP}] [{State}] Packet Id {PacketId} is not yet implemented")] 
    private partial void LogPacketNotImplemented(string? ip, ConnectionState state, int packetId);
}