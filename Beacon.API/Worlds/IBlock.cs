using Beacon.API.Models;

namespace Beacon.API.Worlds;

public interface IBlock
{
    public IWorld World { get; }
    public Location Location { get; }
}