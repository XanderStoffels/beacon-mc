using System.Buffers;
using System.Buffers.Binary;
using Beacon.Util;

namespace Beacon.Net.Packets.Status.ServerBound;


/// <summary>
/// Represents a ping request packet from the client to the server.
/// This packet does not follow the general Beacon way of packet handling; after receiving this packet, the connection
/// immediately sends a Pong packet back to the client instead of letting the game loop handle it.
/// </summary>
public class PingRequest : Rentable<PingRequest>, IServerBoundPacket
{
    public const int PacketId = 0x01;
    
    /// <summary>
    /// Field Type: Long <br/>
    /// May be any number, but vanilla clients use will always use the timestamp in milliseconds.
    /// </summary>
    public long Timestamp { get; set; }
    
    public void Handle(Server server, Connection connection)
    {
        // Nothing to do here, the connection will handle this logic itself.
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        if (!reader.TryReadLong(out var timestamp)) return false;
        
        Timestamp = timestamp;
        return true;
    }

    public static void WritePong(Stream stream, long timeStamp)
    {
        // This packet has a fixed length of 10 bytes.
        const int packetLength = 10;
        const int payloadLength = 9;
        Span<byte> buffer = stackalloc byte[packetLength];
        
        var span = buffer;
        VarInt.TryWrite(span, payloadLength, out _);
        span = span[1..]; // VarInt with value 9 is 1 byte long.
        
        VarInt.TryWrite(span, PacketId, out _);
        span = span[1..]; // PacketId is 1, which is always 1 byte long.
        
        // Write the timestamp which is always 8 bytes.
        BinaryPrimitives.WriteInt64BigEndian(span, timeStamp);
        
        // Write to the stream.
        stream.Write(buffer);
    }
    
}