using Beacon.API.Worlds;

namespace Beacon.API.Models
{
    public record Location(IWorld World, Vector Vector, double Pitch, double Yaw)
    {
        public Location Add(Location other) => this + other;
        public Location Subtract(Location other) => this - other;
        public Location Multiply(Location other) => this * other;
        public double Distance(Location other) => Vector.Distance(other.Vector);  
        public int GetBlockX() => (int)Math.Floor(Vector.X);
        public int GetBlockY() => (int)Math.Floor(Vector.X);
        public int GetBlockZ() => (int)Math.Floor(Vector.X);

        public static Location operator +(Location a, Location b) => a with { Vector = a.Vector.Add(b.Vector) };
        public static Location operator -(Location a, Location b) => a with { Vector = a.Vector.Subtract(b.Vector) };
        public static Location operator *(Location a, Location b) => a with { Vector = a.Vector.Multiply(b.Vector) };


    }
}
