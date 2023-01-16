namespace Beacon.API.Worlds;

public interface IWorld
{
    public string Name { get;}
    
    /**
     * Gets a chunk in this world. If it is not loaded, it will be loaded. It if is not generated, it will be generated.
     */
    public ValueTask<IChunk> GetChunkAsync(int chunkX, int chunkZ, CancellationToken cancelToken);

    
}