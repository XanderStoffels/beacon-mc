using System.Buffers;
using System.Runtime.CompilerServices;

namespace Beacon.Server.Utils;

/// <remarks>Thanks to the Obsidian team. Makes my live easier.</remarks>
/// <typeparam name="T"></typeparam>
public class RentedArray<T> : IDisposable
{
    public int Length { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; }
    public Span<T> Span => array.AsSpan(0, Length);
    public Memory<T> Memory => array.AsMemory(0, Length);
    
    private readonly ArrayPool<T> pool;
    private readonly T[] array;
    

    public RentedArray(int length, ArrayPool<T>? pool = null)
    {
        this.pool = pool ?? ArrayPool<T>.Shared;
        array = this.pool.Rent(length);
        Length = length;
    }

    /// <summary>
    /// Returns the rented array to the pool
    /// </summary>
    public void Dispose() => pool.Return(array);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Span<T>(in RentedArray<T> rentedArray) => rentedArray.Span;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(in RentedArray<T> rentedArray) => rentedArray.Span;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Memory<T>(in RentedArray<T> rentedArray) => rentedArray.Memory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlyMemory<T>(in RentedArray<T> rentedArray) => rentedArray.Memory;
}