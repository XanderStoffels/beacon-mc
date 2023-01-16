namespace Beacon.API.Worlds;

public interface IChunk
{
    public int X { get; }
    public int Z { get; }
    public IWorld World { get; }
    
    public static (int ChunkX, int ChunkZ) GetChunkCoordinates(int worldX, int worldZ)
    {
        return ((int)Math.Floor(worldX / 16f), (int)Math.Floor(worldZ / 16f));
    }
}