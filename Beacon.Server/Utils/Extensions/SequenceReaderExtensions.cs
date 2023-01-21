using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace Beacon.Server.Utils;

public static class SequenceReaderExtensions
{
    /// <summary>
    /// Try reading the next VarInt as defined in the Minecraft Protocol. Advances the reader by the number of bytes read (bytesRead).
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="result">The resulting VarInt</param>
    /// <param name="bytesRead">The size of the VarInt in bytes, thus the amount of bytes the reader has advanced.</param>
    /// <remarks>A VarInt is between 1 and 5 bytes long. Important: if a VarInt could not be parsed, the reader can still have advanced by maximum 5 bytes.
    /// You can check bytesRead to see how many places the reader has advanced.</remarks>
    /// <returns>Indicator if there is enough data to read a VarInt</returns>
    public static bool TryReadVarInt(this ref SequenceReader<byte> reader, out int result, out int bytesRead)
    {
        result = default;
        bytesRead = default;
        if (reader.Length == 0) return false;

        while (reader.TryRead(out var nextByte))
        {
            var shift = 7 * bytesRead++;
            result |= nextByte & 0x7F << shift;
            if ((nextByte & 0x80) == 0) return true;
            if (shift >= 32) throw new("VarInt is too big");
        }
        return false;
    }
    

    /// <summary>
    /// Try reading the next VarLong as defined in the Minecraft Protocol. Advances the reader by the number of bytes read (bytesRead).
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="result">The resulting VarLong</param>
    /// <param name="bytesRead">The size of the VarLong in bytes, thus the amount of bytes the reader has advanced.</param>
    /// <remarks>A VarLong is between 1 and 10 bytes long. Important: if a VarLong could not be parsed, the reader can still have advanced by maximum 10 bytes.
    /// You can check bytesRead to see how many places the reader has advanced.</remarks>
    /// <returns>Indicator if there is enough data to read a VarLong</returns>
    public static bool TryReadVarLong(this ref SequenceReader<byte> reader, out long result, out int bytesRead)
    {
        result = default;
        bytesRead = default;
        if (reader.Remaining == 0) return false;

        while (reader.TryRead(out var nextByte))
        {
            var shift = 7 * bytesRead++;
            result |= (long)nextByte & 0x7F << shift;
            if ((nextByte & 0x80) == 0) return true;
            if (shift >= 64) throw new("VarLong is too big");
        }
        return false;
    }
    
    /// <summary>
    /// Tries to read the next string from the SequenceReader as defined in the Minecraft Protocol.
    /// A string is prefixed by a VarInt, indicating it's size.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="result">The resulted string</param>
    /// <param name="bytesRead"></param>
    /// <param name="maxStringLength">The maximum amount of characters (not bytes) the string should have.</param>
    /// <returns>Indicator if there is enough data to read a VarInt and a string the size of the VarInt's value</returns>
    /// <remarks>The string should be UTF-8 encoded.</remarks>
    /// <exception cref="IOException">If the string exceeds the provided max length.</exception>
    public static bool TryReadString(this ref SequenceReader<byte> reader, out string result, out int bytesRead, int maxStringLength = 32767)
    {
        result = string.Empty;
        bytesRead = 0;
        
        if (!reader.TryReadVarInt(out var strLength, out bytesRead))
            return false;

        if (!reader.TryReadExact(strLength, out var stringBytes))
            return false;
        bytesRead += strLength;
        
        result = Encoding.UTF8.GetString(stringBytes);
        if (maxStringLength > 0 && result.Length > maxStringLength)
            throw new IOException($"string ({result.Length}) exceeded maximum length ({maxStringLength})");

        return true;
    }

    /// <summary>
    /// Tries to read a 2-byte unsigned short.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="result"></param>
    /// <param name="bytesRead"></param>
    /// <returns>Indicator if there is enough data to read a ushort</returns>
    public static bool TryReadUnsignedShort(this ref SequenceReader<byte> reader, out ushort result, out int bytesRead)
    {
        result = 0;
        bytesRead = 0;

        if (!reader.TryRead(out var byte1))
            return false;
        bytesRead++;
        
        if (!reader.TryRead(out var byte2))
            return false;
        bytesRead++;
        
        result = (ushort)((byte1 << 8) | byte2);
        return true;
    }
    
}