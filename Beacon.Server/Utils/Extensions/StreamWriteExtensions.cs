using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Beacon.Server.Utils.Extensions;

internal static class StreamWriteExtensions
{
    public static void WriteLong(this Stream stream, long value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        stream.Write(buffer);
    }
    
    public static async Task WriteLongAsync(this Stream stream, long value)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(8);
        BinaryPrimitives.WriteInt64BigEndian(buffer.AsSpan(0, 8), value);
        await stream.WriteAsync(buffer, 0, 8);
    }

    public static void WriteUnsignedShort(this Stream stream, ushort value)
    {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(span, value);
        stream.Write(span);
    }

    public static void WriteString(this Stream stream, string value)
    {
        /*
        var length = Encoding.UTF8.GetByteCount(value);
        var bytes = ArrayPool<byte>.Shared.Rent(length);
        Encoding.UTF8.GetBytes(value, bytes);
        stream.WriteVarInt(length);
        stream.Write(bytes.AsSpan(0, length));
        ArrayPool<byte>.Shared.Return(bytes);
        */ 
        var bytes = new byte[Encoding.UTF8.GetByteCount(value)];
        Encoding.UTF8.GetBytes(value, bytes.AsSpan());
        stream.WriteVarInt(bytes.Length);
        stream.Write(bytes);
    }
    
    public static async Task WriteStringAsync(this Stream stream, string value)
    {
        var length = Encoding.UTF8.GetByteCount(value);
        var bytes = ArrayPool<byte>.Shared.Rent(length);
        Encoding.UTF8.GetBytes(value, bytes);
        await stream.WriteVarIntAsync(length);
        await stream.WriteAsync(bytes.AsMemory(0, length));
        ArrayPool<byte>.Shared.Return(bytes);
    }

    public static void WriteVarInt(this Stream stream, int value)
    {
        var unsigned = (uint)value;

        do
        {
            var temp = (byte)(unsigned & 127);
            unsigned >>= 7;

            if (unsigned != 0)
                temp |= 128;

            stream.WriteByte(temp);
        } while (unsigned != 0);
    }
    
    public static async Task WriteVarIntAsync(this Stream stream, int value)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(5);
        var index = 0;
        var unsigned = (uint)value;

        do
        {
            var temp = (byte)(unsigned & 127);
            unsigned >>= 7;

            if (unsigned != 0)
                temp |= 128;

            buffer[index++] = temp;
        } while (unsigned != 0);

        await stream.WriteAsync(buffer, 0, index);
        ArrayPool<byte>.Shared.Return(buffer);
    }

    public static void WriteVarLong(this Stream stream, long value)
    {
        var unsigned = (ulong)value;

        do
        {
            var temp = (byte)(unsigned & 127);

            unsigned >>= 7;

            if (unsigned != 0)
                temp |= 128;


            stream.WriteByte(temp);
        } while (unsigned != 0);
    }

    public static async Task WriteVarLongAsync(this Stream stream, long value)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(10);
        var index = 0;
        var unsigned = (ulong)value;

        do
        {
            var temp = (byte)(unsigned & 127);

            unsigned >>= 7;

            if (unsigned != 0)
                temp |= 128;

            buffer[index++] = temp;
        } while (unsigned != 0);

        await stream.WriteAsync(buffer, 0, index);
        ArrayPool<byte>.Shared.Return(buffer);
    }
}