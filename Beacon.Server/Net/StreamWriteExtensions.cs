using System.Buffers.Binary;
using System.Text;

namespace Beacon.Server.Net;

internal static class StreamWriteExtensions
{
    public static void WriteLong(this Stream stream, ulong value)
    {
        Span<byte> buffer = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        stream.Write(buffer);
    }

    public static void WriteUnsignedShort(this Stream stream, ushort value)
    {
        Span<byte> span = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(span, value);
        stream.Write(span);
    }

    public static void WriteString(this Stream stream, string value)
    {
        var bytes = new byte[Encoding.UTF8.GetByteCount(value)];
        Encoding.UTF8.GetBytes(value, bytes.AsSpan());
        stream.WriteVarInt(bytes.Length);
        stream.Write(bytes);
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
}