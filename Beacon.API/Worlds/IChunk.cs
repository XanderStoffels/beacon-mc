using Beacon.API.Models;

namespace Beacon.API.Worlds;

public interface IChunk
{
    public IWorld World { get; }
    public Location Location { get; }
    public bool IsLoaded { get; }
}