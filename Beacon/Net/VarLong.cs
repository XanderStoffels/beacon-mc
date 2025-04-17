namespace Beacon.Net;

public static class VarLong
{
    public const int MinSize = 1;
    public const int MaxSize = 10;
    
    /// <summary>
    /// Get the size of a VarLong in bytes.
    /// All VarLongs are between 1 and 10 bytes.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int GetSize(long value)
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
    /// Convert a given long to a VarLong and write it to the given Span.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="value"></param>
    /// <param name="written">The amount of bytes written to the Span.</param>
    /// <returns>The amount of bytes written to the Span.</returns>
    public static bool TryWrite(Span<byte> buffer, long value, out int written)
    {
        written = 0;
        do
        {
            if (written >= buffer.Length)
                return false;

            byte temp = (byte)(value & 0b0111_1111);
            value >>= 7;
            if (value != 0)
                temp |= 0b1000_0000;

            buffer[written++] = temp;
        } while (value != 0);

        return true;
    }
}