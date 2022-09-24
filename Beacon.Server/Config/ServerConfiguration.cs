using Beacon.API.Util;

namespace Beacon.Server.Config;

public class ServerConfiguration : IServerConfiguration
{
    public int Port { get; } = 25565;
    public string MOTD { get; } = "A Beacon Server!";
    public int MaxPlayers { get; } = 10;
}