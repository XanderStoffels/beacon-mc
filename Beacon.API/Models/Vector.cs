﻿namespace Beacon.API.Models;

public record Vector(double X, double Y, double Z)
{
    public Vector Add(Vector other)
    {
        return this + other;
    }

    public Vector Subtract(Vector other)
    {
        return this - other;
    }

    public Vector Multiply(Vector other)
    {
        return this * other;
    }

    public double Distance(Vector other)
    {
        return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2) + Math.Pow(other.Z - Z, 2));
    }

    public static Vector operator +(Vector a, Vector b)
    {
        return a with { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z };
    }

    public static Vector operator -(Vector a, Vector b)
    {
        return a with { X = a.X - b.X, Y = a.Y - b.Y, Z = a.Z - b.Z };
    }

    public static Vector operator *(Vector a, Vector b)
    {
        return a with { X = a.X * b.X, Y = a.Y * b.Y, Z = a.Z * b.Z };
    }
}