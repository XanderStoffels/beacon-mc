using Beacon.Util;

namespace Beacon.Net.Packets;

public interface IClientBoundPacket 
{
    /// <summary>
    /// Try to write the payload of a packet to a given buffer.
    /// The payload is all information in the packet that follows the initial total length of the packet.
    /// The total length of the packet will not be written to the buffer.
    /// </summary>
    /// <param name="buffer">The buffer to write to.</param>
    /// <param name="bytesWritten">The amount of bytes written to the buffer.</param>
    /// <returns>True if the buffer was big enough, false otherwise.</returns>
    public bool SerializePayload(Span<byte> buffer, out int bytesWritten);
    
}