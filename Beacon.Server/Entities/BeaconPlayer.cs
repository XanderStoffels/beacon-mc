using Beacon.API;
using Beacon.API.Entities;
using Beacon.API.Models;
using Beacon.API.Worlds;

namespace Beacon.Server.Entities;

internal class BeaconPlayer : IPlayer
{
    public IWorld World => throw new NotImplementedException();
    public IServer Server => throw new NotImplementedException();

    public ValueTask Destroy()
    {
        throw new NotImplementedException();
    }

    public Task SendMessageAsync(string message)
    {
        throw new NotImplementedException();
    }

    public ValueTask TeleportTo(Location location)
    {
        throw new NotImplementedException();
    }
}