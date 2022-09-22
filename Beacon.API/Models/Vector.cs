namespace Beacon.API.Models
{
    public record Vector(double X, double Y, double Z)
    {
        public Vector Add(Vector other) => this + other;
        public Vector Subtract(Vector other) => this - other;
        public Vector Multiply(Vector other) => this * other;
        public double Distance(Vector other) => Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2) + Math.Pow(other.Z - Z, 2));

        public static Vector operator +(Vector a, Vector b) => a with { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
        public static Vector operator -(Vector a, Vector b) => a with { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
        public static Vector operator *(Vector a, Vector b) => a with { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Z * b.Z };
    }
}
