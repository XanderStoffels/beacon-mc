namespace Beacon.Server.Worlds.Regions;

/// <summary>
/// A Region manager that loads regions from the local file system.
/// </summary>
public class LocalRegionManager : IRegionManager
{
    private readonly DirectoryInfo _worldDirectory;
    public LocalRegionManager(DirectoryInfo worldDirectory)
    {
        _worldDirectory = worldDirectory;
    }
    
    public Task<IRegion> GetRegionAsync(int x, int y, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }
}