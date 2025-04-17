using System.Buffers;

namespace Beacon.Net.Packets;

public interface IPipeReadable<out T>
{
    public static abstract T Deserialize(ref SequenceReader<byte> reader);
}