using Beacon.API.Worlds;

namespace Beacon.Server.Worlds.Regions;

public interface IRegion
{
    public ValueTask<IChunk> GetChunkAsync(int chunkX, int chunkZ, CancellationToken cancelToken);
}