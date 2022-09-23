using Beacon.API.Models;
using Beacon.API.Worlds;

namespace Beacon.API.Entities;

public interface IEntity
{
    public IWorld World { get; }
    public ValueTask TeleportTo(Location location);
    public ValueTask Destroy();
}