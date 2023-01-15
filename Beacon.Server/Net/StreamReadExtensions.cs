using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Beacon.Server.Net;

public static class StreamReadExtensions
{
    private static byte[] Rent(int minSize) => ArrayPool<byte>.Shared.Rent(minSize);
    private static (byte[] buffer, Memory<byte> span) RentWithMemory(int size)
    {
        var buffer = Rent(size);
        return (buffer, buffer.AsMemory(0, size));
    }
    private static void Return(byte[] array) => ArrayPool<byte>.Shared.Return(array);
    
    public static ulong ReadLong(this Stream stream)
    {
        var buffer = Rent(8);
        var span = buffer.AsSpan(0, 8);
        stream.ReadExactly(span);
        var r = BinaryPrimitives.ReadUInt64BigEndian(span);
        
        Return(buffer);
        return r;
    }

    public static ushort ReadUnsignedShort(this Stream stream)
    {
        var buffer = Rent(2);
        var span = buffer.AsSpan(0, 2);
        stream.ReadExactly(span);
        var r = BinaryPrimitives.ReadUInt16BigEndian(span);
        
        Return(buffer);
        return r;
    }
    
    public static string ReadString(this Stream stream, int maxLength = 32767)
    {
        var (strLength, _) = stream.ReadVarInt();
        var buffer = Rent(strLength);
        var span = buffer.AsSpan(0, strLength);
        stream.ReadExactly(span);

        var value = Encoding.UTF8.GetString(span);
        if (maxLength > 0 && value.Length > maxLength)
            throw new IOException($"string ({value.Length}) exceeded maximum length ({maxLength})");

        Return(buffer);
        return value;
    }
    
    public static async Task<string> ReadStringAsync(this Stream stream, int maxLength = 32767)
    {
        var (strLength, _) = await stream.ReadVarIntAsync();
        var (buffer, memory) = RentWithMemory(strLength);
        await stream.ReadExactlyAsync(memory);

        var value = Encoding.UTF8.GetString(memory.Span);
        if (maxLength > 0 && value.Length > maxLength)
            throw new IOException($"string ({value.Length}) exceeded maximum length ({maxLength})");

        Return(buffer);
        return value;
    }

    public static byte ReadUnsignedByte(this Stream stream)
    {
        var buffer = Rent(1);
        stream.ReadExactly(buffer, 0, 1);
        var r = buffer[0];
        
        Return(buffer);
        return r;
    }

    public static async ValueTask<byte> ReadUnsignedByteAsync(this Stream stream)
    {
        var buffer = Rent(1);
        await stream.ReadExactlyAsync(buffer, 0, 1);
        var r = buffer[0];
        
        Return(buffer);
        return buffer[0];
    }

    public static (int value, int bytesRead) ReadVarInt(this Stream stream)
    {
        var numRead = 0;
        var result = 0;
        byte read;
        do
        {
            read = stream.ReadUnsignedByte();
            var value = read & 0b01111111;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5) throw new InvalidOperationException("VarInt is too big");
        } while ((read & 0b10000000) != 0);

        return (result, numRead);
    }

    public static async ValueTask<(int value, int bytesRead)> ReadVarIntAsync(this Stream stream)
    {
        var numRead = 0;
        var result = 0;
        byte read;
        do
        {
            read = await stream.ReadUnsignedByteAsync();
            var value = read & 0b01111111;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5) throw new InvalidOperationException("VarInt is too big");
        } while ((read & 0b10000000) != 0);

        return (result, numRead);
    }

    public static (long value, int bytesRead) ReadVarLong(this Stream stream)
    {
        var numRead = 0;
        long result = 0;
        byte read;
        do
        {
            read = stream.ReadUnsignedByte();
            var value = read & 0b01111111;
            result |= (long)value << (7 * numRead);

            numRead++;
            if (numRead > 10) throw new InvalidOperationException("VarLong is too big");
        } while ((read & 0b10000000) != 0);

        return (result, numRead);
    }
    
    public static async Task<ushort> ReadUnsignedShortAsync(this Stream stream)
    {
        var buffer = Rent(2);
        await stream.ReadExactlyAsync(buffer, 0, 2);
        var result = BinaryPrimitives.ReadUInt16BigEndian(buffer);
        Return(buffer);
        return result;
    }

    public static async ValueTask<(long value, int bytesRead)> ReadVarLongAsync(this Stream stream,
        CancellationToken cToken = default)
    {
        var numRead = 0;
        long result = 0;
        byte read;
        do
        {
            read = await stream.ReadUnsignedByteAsync();
            var value = read & 0b01111111;
            result |= (long)value << (7 * numRead);

            numRead++;
            if (numRead > 10) throw new InvalidOperationException("VarLong is too big");
        } while ((read & 0b10000000) != 0);

        return (result, numRead);
    }
}