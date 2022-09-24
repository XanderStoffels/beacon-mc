using Beacon.API;
using Beacon.API.Models;
using Beacon.Server.Utils;

namespace Beacon.Server;

public sealed partial class BeaconServer : IServer
{
    public ServerStatus GetStatus()
    {
        return new ServerStatus
        {
            Version = new ServerVersionModel
            {
                Name = "1.18.1",
                Protocol = 757
            },
            Players = new OnlinePlayersModel
            {
                Max = _env.ServerConfiguration.MaxPlayers,
                Online = 1,
                Sample = new[]
                {
                    new OnlinePlayerModel
                    {
                        Name = "Notch",
                        Id = new Guid().ToString()
                    }
                }
            },
            Description = new DescriptionModel
            {
                Text = _env.ServerConfiguration.MOTD
            },
            Favicon = Resources.ServerIcon
        };
    }
}