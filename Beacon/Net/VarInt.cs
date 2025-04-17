namespace Beacon.Net;

public static class VarInt
{
    public const int MinSize = 1;
    public const int MaxSize = 5;
    
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

