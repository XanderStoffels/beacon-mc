using Beacon.API.Entities;
using Beacon.API.Models;
using Beacon.API.Worlds;

namespace Beacon.Server
{
    internal class BeaconPlayer : IPlayer
    {
        public IWorld World => throw new NotImplementedException();

        public ValueTask Destroy()
        {
            throw new NotImplementedException();
        }

        public ValueTask TeleportTo(Location location)
        {
            throw new NotImplementedException();
        }
    }
}
