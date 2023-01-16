namespace Beacon.Server.Worlds;

/*
 * A world is a collection of regions (infinite).
 * A region is a collection of columns (32x1x32 columns).
 * A column is a collection of chunks (1x16x1 chunks).
 *
 * A chunk stores four (or five) things:
 *  block IDs (8-bit)
 *  block metadata (4-bit)
 *  block light (4-bit)
 *  sky light (4-bit)
 *  optional 'add data' which is four bits to be added to block IDs for additional block ID support
 */

public class AnvilWorldReader
{
    private const string RegionFileFormat = "r.{X}.{Z}.mca";

    private static (int X, int Z) ChunkCordsToRegionCords(int x, int z) => (x / 32, z / 32);
        
}