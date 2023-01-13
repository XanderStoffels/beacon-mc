namespace Beacon.API.Worlds.Generators;

public interface IWorldGenerator
{
    BlockType GetBlock(int x, int y, int z);
}