namespace Beacon.Server.Net;

public enum QueueAndParseResult
{
    Ok,
    NeedMoreData,
    InvalidPacket,
    CouldNotQueue
}