using Beacon.API.Worlds;

namespace Beacon.API.Models;

public record Location(IWorld World, Vector Vector, double Pitch, double Yaw)
{
    public Location Add(Location other)
    {
        return this + other;
    }

    public Location Subtract(Location other)
    {
        return this - other;
    }

    public Location Multiply(Location other)
    {
        return this * other;
    }

    public double Distance(Location other)
    {
        return Vector.Distance(other.Vector);
    }

    public int GetBlockX()
    {
        return (int)Math.Floor(Vector.X);
    }

    public int GetBlockY()
    {
        return (int)Math.Floor(Vector.X);
    }

    public int GetBlockZ()
    {
        return (int)Math.Floor(Vector.X);
    }

    public static Location operator +(Location a, Location b)
    {
        return a with { Vector = a.Vector.Add(b.Vector) };
    }

    public static Location operator -(Location a, Location b)
    {
        return a with { Vector = a.Vector.Subtract(b.Vector) };
    }

    public static Location operator *(Location a, Location b)
    {
        return a with { Vector = a.Vector.Multiply(b.Vector) };
    }
}