namespace Beacon.Server.Worlds.Regions;

public interface IRegionManager
{
    Task<IRegion> GetRegionAsync(int x, int y, CancellationToken cancelToken);
}