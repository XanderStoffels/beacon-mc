using System.Buffers.Binary;
using System.Text;

namespace Beacon.Server.Net;

// Thanks to Obsidian! 
// https://github.com/ObsidianMC/Obsidian

public static class StreamReadExtensions
{
    public static ulong ReadLong(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[8];
        stream.Read(buffer);
        return BinaryPrimitives.ReadUInt64BigEndian(buffer);
    }

    public static ushort ReadUnsignedShort(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[2];
        stream.Read(buffer);
        return BinaryPrimitives.ReadUInt16BigEndian(buffer);
    }

    public static string ReadString(this Stream stream, int maxLength = 32767)
    {
        var (length, _) = stream.ReadVarInt();
        var buffer = new byte[length];
        stream.Read(buffer, 0, length);

        var value = Encoding.UTF8.GetString(buffer);
        if (maxLength > 0 && value.Length > maxLength)
            throw new IOException($"string ({value.Length}) exceeded maximum length ({maxLength})");

        return value;
    }

    public static byte ReadUnsignedByte(this Stream stream)
    {
        Span<byte> buffer = stackalloc byte[1];
        stream.Read(buffer);
        return buffer[0];
    }

    public static async ValueTask<byte> ReadUnsignedByteAsync(this Stream stream)
    {
        var buffer = new byte[1];
        await stream.ReadAsync(buffer);
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