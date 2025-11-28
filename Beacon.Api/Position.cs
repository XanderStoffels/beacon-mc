namespace Beacon.Api;

/// <summary>
/// x as a 26-bit integer, followed by z as a 26-bit integer, followed by y as a 12-bit integer (all signed, two's complement).
/// x (-33554432 to 33554431), z (-33554432 to 33554431), y (-2048 to 2047)
/// </summary>
public readonly struct Position
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;
    
    public Position(int x, int y, int z)
    {
        if (x is < -33554432 or > 33554431)
            throw new ArgumentOutOfRangeException(nameof(x), "X must be between -33554432 and 33554431");
        
        if (z is < -33554432 or > 33554431)
            throw new ArgumentOutOfRangeException(nameof(z), "Z must be between -33554432 and 33554431");
        
        if (y is < -2048 or > 2047)
            throw new ArgumentOutOfRangeException(nameof(y), "Y must be between -2048 and 2047");
        
        X = x;
        Y = y;
        Z = z;
    }

    public long ToLong()
    {
        // 64-bit value split into three signed integer parts:
        // x: 26 MSBs
        // z: 26 middle bits
        // y: 12 LSBs
        return (((long)X & 0x3FFFFFF) << 38) | (((long)Z & 0x3FFFFFF) << 12) | ((long)Y & 0xFFF);
    }
    
    public static bool TryParse(long value, out Position position)
    {
        var x = (int)((value >> 38) & 0x3FFFFFF);
        var y = (int)((value >> 12) & 0xFFF);
        var z = (int)(value & 0x3FFFFFF);
        
        if (x is < -33554432 or > 33554431)
        {
            position = default;
            return false;
        }
        
        if (z is < -33554432 or > 33554431)
        {
            position = default;
            return false;
        }
        
        if (y is < -2048 or > 2047)
        {
            position = default;
            return false;
        }
        
        position = new Position(
            (int)((value >> 38) & 0x3FFFFFF),
            (int)((value >> 12) & 0xFFF),
            (int)(value & 0x3FFFFFF));
        
        return true;
    }
}