using System.Buffers;
using Beacon.Server.Utils;

namespace Beacon.Server.Worlds;

/// <summary>
/// Represents a physical region file.
/// </summary>
public class RegionFile
{

    private readonly Memory<byte> _locationTable;
    private readonly Memory<byte> _lastModifiedTable;
    private readonly Memory<byte> _chunkData;
     
    public RegionFile(Memory<byte> locationTable, Memory<byte> lastModifiedTable, Memory<byte> chunkData)
    {
        _locationTable = locationTable;
        _lastModifiedTable = lastModifiedTable;
        _chunkData = chunkData;
    }

    public static async Task<RegionFile> From(FileInfo fileInfo)
    {
        if (!fileInfo.Exists)
            throw new FileNotFoundException("RegionFile file not found", fileInfo.FullName);

        if (fileInfo.Length > int.MaxValue)
            throw new ArgumentException("RegionFile file is too large", nameof(fileInfo));

        using var buffer = new RentedArray<byte>((int)fileInfo.Length);
        await using var fileStream = fileInfo.OpenRead();
        await fileStream.ReadExactlyAsync(buffer.Memory);
        return From(buffer.Memory);
    }

    public static RegionFile From(Memory<byte> memory)
        => new(memory[..(4*1024)], 
            memory[(4*1024)..(8*1024)], 
            memory[(8*1024)..]);
    
}