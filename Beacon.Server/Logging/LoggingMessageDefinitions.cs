using Beacon.Server.Net;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Logging;

public static partial class LoggingMessageDefinitions
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "[{IP}] [{State}] Pipe read was canceled from writer")]
    public static partial void LogPipeCanceledFromWriter(this ILogger logger, string? ip,
        ConnectionState state);
}