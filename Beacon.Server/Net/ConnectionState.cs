namespace Beacon.Server.Net;

public enum ConnectionState
{
    Handshaking,
    Status,
    Login,
    Play
}