namespace Beacon.Net;

public static class VarInt
{
    public const int MinSize = 1;
    public const int MaxSize = 5;
    private const int SegmentBits = 0x7F;
    private const int ContinueBit = 0x80;
    
    /// <summary>
    /// Try reading the next VarInt as defined in the Minecraft Protocol.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value">The resulting VarInt</param>
    /// <param name="bytesRead">How many bytes from the span make up the read VarInt.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool TryRead(ReadOnlySpan<byte> buffer, out int value, out byte bytesRead)
    {
        value = 0;
        bytesRead = 0;
        if (buffer.Length < MinSize) return false;
        
        while (true)
        {
            var currentByte = buffer[bytesRead];
            value |= (currentByte & SegmentBits) << bytesRead * 7;
            bytesRead++;

            // Check if the most significant bit is set. If so, the next byte is part of the VarInt.
            if ((currentByte & ContinueBit) == 0) break;
            if (bytesRead <= MaxSize) continue;
            
            // If we reach this point, the VarInt is too long.
            value = 0;
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Get the size of a VarInt in bytes.
    /// The size will always be between 1 and 5 bytes.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int GetSize(int value)
    {
        var size = 0;
        do
        {
            value >>= 7;
            size++;
        } while (value != 0);
        return size;
    }

    /// <summary>
    /// Convert a given int to a VarInt and write it to the given Span.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value"></param>
    /// <param name="written">How many bytes were written to the span.</param>
    /// <returns>If the operation was successful.</returns>
    public static bool TryWrite(Span<byte> buffer, int value, out int written)
    {
        written = 0;
        do
        {
            if (written >= buffer.Length)
                return false;

            var temp = (byte)(value & 0b0111_1111);
            value >>= 7;
            if (value != 0)
                temp |= 0b1000_0000;

            buffer[written++] = temp;
        } while (value != 0);

        return true;
    }
}

