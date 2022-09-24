namespace Beacon.API.Util;

public interface IServerConfiguration
{
    public int Port { get; }
    public string MOTD { get; }
    public int MaxPlayers { get; }
}