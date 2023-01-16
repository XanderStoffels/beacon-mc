using Beacon.API.Worlds;
using Beacon.Server.Worlds.Regions;

namespace Beacon.Server.Worlds;

public class World : IWorld
{
    private readonly IRegionManager _regionManager;
    public string Name { get; }

    public World(string worldName)
    {
        Name = worldName;
        var worldsDir = new DirectoryInfo("worlds");
        var worldDir = Path.Combine(worldsDir.FullName, Name);
        _regionManager = new LocalRegionManager(new(worldDir));
    }
    
    public World(string name, IRegionManager regionManager)
    {
        Name = name;
        _regionManager = regionManager;
    }
    
    
    public async ValueTask<IChunk> GetChunkAsync(int chunkX, int chunkZ, CancellationToken cancelToken = default)
    {
        var (regionX, regionZ) = ChunkToRegionCoordinate(chunkX, chunkZ);
        var region = await _regionManager.GetRegionAsync(regionX, regionZ, cancelToken);
        return await region.GetChunkAsync(chunkX, chunkZ, cancelToken);
    }

    private async ValueTask<IRegion> GetRegionAsync(int worldX, int worldZ, CancellationToken cancelToken)
    {
        var (chunkX, chunkZ) = IChunk.GetChunkCoordinates(worldX, worldZ);
        var (regionX, regionZ) = ChunkToRegionCoordinate(chunkX, chunkZ);
        return await _regionManager.GetRegionAsync(regionX, regionZ, cancelToken);
    }

    private static (int RegionX, int RegionZ) ChunkToRegionCoordinate(int chunkX, int chunkZ)
        => ((int)Math.Floor(chunkX / 32f), (int)Math.Floor(chunkZ / 32f));
    
}