namespace Beacon.API.Worlds;

public interface IWorld
{
    public string Name { get; }
    public ValueTask<IBlock> GetBlockAt();
}