namespace Beacon.Server.Utils;

public static class ByteMemoryExtensions
{
    public static int GetVarInt(this ReadOnlySpan<byte> span)
    {
        var index = 0;
        var numRead = 0;
        var result = 0;
        byte read;
        
        do
        {
            if (span.Length < index - 1)
            {
                throw new Exception("Not enough bytes to read VarInt");
            }
            read = span[index++];
            var value = read & 0b01111111;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5) throw new InvalidOperationException("VarInt is too big");
        } while ((read & 0b10000000) != 0);
        return result;
    }    
}