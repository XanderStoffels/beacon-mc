using System.Text;

namespace Beacon.Server.Utils.Extensions;

public static class MinecraftValueExtensions
{
    public static int GetVarIntLength(this int value)
    {
        return (System.Numerics.BitOperations.LeadingZeroCount((uint)value | 1) - 38) * -1171 >> 13;
    }
    
    public static int GetVarLongLength(this long value)
    {
        return (System.Numerics.BitOperations.LeadingZeroCount((ulong)value | 1) - 70) * -1171 >> 13;    
    }

    public static int GetVarStringLength(this string value)
    {
        var strByteLength = Encoding.UTF8.GetByteCount(value);
        return strByteLength.GetVarIntLength() + strByteLength;
    }
}