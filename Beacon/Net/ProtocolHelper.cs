using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Beacon.Net;

public static class ProtocolHelper
{
    private const int SegmentBits = 0x7F;
    private const int ContinueBit = 0x80;
    
    /// <summary>
    /// Read a VarInt from the given Span.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="bytesRead">How many bytes from the span make up the read VarInt.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static int ReadVarInt(ReadOnlySpan<byte> buffer, out byte bytesRead)
    {
        if (buffer.Length == 0)
            throw new InvalidOperationException("Buffer is empty");

        var value = 0;
        byte position = 0;

        while (true)
        {
            var currentByte = buffer[position];
            value |= (currentByte & SegmentBits) << position * 7;
            position++;

            // Check if the most significant bit is set. If so, the next byte is part of the VarInt.
            if ((currentByte & ContinueBit) == 0) break;
            if (position > 5) throw new InvalidOperationException("VarInt is too long");
        }

        bytesRead = position;
        return value;
    }
    
    /// <summary>
    /// Read a VarLong from the given Span.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="bytesRead">How many bytes from the span make up the read VarLong</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static long ReadVarLong(ReadOnlySpan<byte> buffer, out byte bytesRead)
    {
        if (buffer.Length == 0)
            throw new InvalidOperationException("Buffer is empty");

        long result = 0;
        byte position = 0;
        while (true)
        {
            var currentByte = buffer[position];
            result |= (long)(currentByte & SegmentBits) << position * 7;
            position++;

            // Check if the most significant bit is set. If so, the next byte is part of the VarLong.
            if ((currentByte & ContinueBit) == 0) break;
            if (position > 10) throw new InvalidOperationException("VarLong is too long");
        }

        bytesRead = position;
        return result;
    }
    
    /// <summary>
    /// Try reading the next VarInt as defined in the Minecraft Protocol. Advances the reader by the number of bytes read (bytesRead).
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="result">The resulting VarInt</param>
    /// <param name="bytesRead">The size of the VarInt in bytes, thus the amount of bytes the reader has advanced.</param>
    /// <remarks>A VarInt is between 1 and 5 bytes long. Important: if a VarInt could not be parsed, the reader can still have advanced by maximum 5 bytes.
    /// You can check bytesRead to see how many places the reader has advanced.</remarks>
    /// <returns>Indicator if a VarInt was able to be read.</returns>
    public static bool TryReadVarInt(this ref SequenceReader<byte> reader, out int result, out int bytesRead)
    {
        result = 0;
        bytesRead = 0;
        if (reader.Length == 0) return false;

        while (reader.TryRead(out var nextByte))
        {
            var shift = 7 * bytesRead++;
            result |= nextByte & 0x7F << shift;
            if ((nextByte & 0x80) == 0) return true;
            if (shift >= 32) return false;
        }
        return false;
    }

    /// <summary>
    /// Try reading the next VarLong as defined in the Minecraft Protocol.
    /// Advances the reader by the number of bytes read (bytesRead).
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="result">The resulting VarLong</param>
    /// <param name="bytesRead">The size of the VarLong in bytes, thus the amount of bytes the reader has advanced.</param>
    /// <remarks>A VarLong is between 1 and 10 bytes long. Important: if a VarLong could not be parsed, the reader can still have advanced by maximum 10 bytes.
    /// You can check bytesRead to see how many places the reader has advanced.</remarks>
    /// <returns>Indicator if a VarLong was able to be read.</returns>
    public static bool TryReadVarLong(this ref SequenceReader<byte> reader, out long result, out int bytesRead)
    {
        result = 0;
        bytesRead = 0;
        if (reader.Remaining == 0) return false;

        while (reader.TryRead(out var nextByte))
        {
            var shift = 7 * bytesRead++;
            result |= (long)nextByte & 0x7F << shift;
            if ((nextByte & 0x80) == 0) return true;
            if (shift >= 64) return false;
        }
        return false;
    }
    
    
    /// <summary>
    /// Try to read a Minecraft string from the given SequenceReader.
    /// The string is expected to be in UTF-8 format and prefixed with a VarInt length.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryReadString(this ref SequenceReader<byte> reader, out string value)
    {
        if (!reader.TryReadVarInt(out var length, out var bytesRead))
        {
            value = string.Empty;
            return false;
        }
        
        if (reader.Length - bytesRead < length)
        {
            value = string.Empty;
            return false;
        }

        var stringBlock = reader.UnreadSpan[..length];
        
        // We need to copy the string block to a new array because the reader will advance and the span will be invalid.
        value = Encoding.UTF8.GetString(stringBlock);
        reader.Advance(length);
        return true;
    }

    /// <summary>
    /// Try to read a <see cref="ushort"/> from the given SequenceReader.
    /// A <see cref="ushort"/> is always 2 bytes.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryReadUShort(this ref SequenceReader<byte> reader, out ushort value)
    {
        // A ushort is 2 bytes long.
        if (reader.Length < 2)
        {
            value = 0;
            return false;
        }
        
        // Read the two bytes and combine them into a ushort.
        value = (ushort)((reader.UnreadSpan[0] << 8) | reader.UnreadSpan[1]);
        reader.Advance(2);
        return true;
    }

    public static bool TryReadLong(this ref SequenceReader<byte> reader, out long value)
    {
        if (reader.TryReadBigEndian(out value)) return true;
        value = 0;
        return false;
    }

    public static bool TryReadUuid(this ref SequenceReader<byte> reader, out Guid value)
    {
        // A Guid requires exactly 16 bytes.
        if (reader.Remaining < 16)
        {
            value = Guid.Empty;
            return false;
        }

        Span<byte> guidBytes = stackalloc byte[16];
        if (!reader.TryCopyTo(guidBytes))
        {
            value = Guid.Empty;
            return false;
        }

        // Advance the reader by 16 bytes.
        reader.Advance(16);

        // Create a Guid from the bytes. Needs to be bigEndian.
        value = new Guid(guidBytes, true);
        return true;
    }

    /// <summary>
    /// Tries to write a <see cref="long"/> to the span.
    /// Always writes 8 bytes.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="buffer"></param>
    /// <returns>False if the span is not big enough to hold 8 bytes, true otherwise.</returns>
    public static bool TryWriteLong(long value, Span<byte> buffer)
    {
        if (buffer.Length < 8)
            return false;
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        return true;
    }
}