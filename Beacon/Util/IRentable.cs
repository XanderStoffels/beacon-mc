namespace Beacon.Util;

/// <summary>
/// An interface for objects that can be rented from an object pool.
/// </summary>
public interface IRentable : IDisposable
{
    /// <summary>
    /// Indicates if the object is currently rented from the object pool.
    /// </summary>
    public bool IsRented { get; }
    
    /// <summary>
    /// Return the object to the object pool.
    /// </summary>
    public void Return();
}

/// <summary>
/// Base class for objects that can be rented from an object pool.
/// Provides a simple implementation of the IRentable over a shared object pool.
/// Automatically sets the IsRented property to true when rented and returns the object to the pool when disposed.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Rentable<T> : IRentable where T : Rentable<T>, IRentable, new()
{
    public bool IsRented { get; private set; }

    /// <summary>
    /// Returns the object to the object pool if it is rented.
    /// </summary>
    public void Return()
    {
        if (!IsRented || this is not T t) return;
        ObjectPool<T>.Shared.Return(t);
        IsRented = false;
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        Return();
    }
    
    /// <summary>
    /// Rents an instance of <see cref="T"/> from the object pool.
    /// </summary>
    /// <returns></returns>
    public static T Rent()
    {
        var o = ObjectPool<T>.Shared.Get();
        o.IsRented = true;
        return o;
    }
}