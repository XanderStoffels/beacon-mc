using System.Buffers;
using System.Text;
using Beacon.Util;

namespace Beacon.Net.Packets.Configuration.ServerBound;

/// <summary>
/// Mods and plugins can use this to send their data. Minecraft itself uses some plugin channels. These internal channels are in the minecraft namespace.
/// More documentation on this: <a href="https://dinnerbone.com/blog/2012/01/13/minecraft-plugin-channels-messaging/">dinnerbone's blogpost</a>.
/// </summary>
/// <remarks>
/// Note that the length of Data is known only from the packet length, since the packet has no length field of any kind.
/// </remarks>
public sealed class CustomPayloadFromClient : Rentable<CustomPayloadFromClient>, IServerBoundPacket
{
    public const int PacketId = 0x02;
    public const int PayloadMaxLength = 32767;

    /// <summary>
    /// Data Type: Identifier
    /// Name of the plugin channel used to send the data.
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Data Type: Byte Array (32767)
    /// Any data, depending on the channel. minecraft: channels are documented <a href="https://minecraft.wiki/w/Minecraft_Wiki:Projects/wiki.vg_merge/Plugin_channels">here</a>.
    /// The length of this array must be inferred from the packet length.
    /// </summary>
    public byte[] Payload { get; set; } = [];
    
    private bool _isPayloadArrayRented;
    
    public void Handle(Server server, Connection connection)
    {
        // There are so many custom payloads, this is probably not the best place to handle them.
        // For now, we only care about the Brand the client sends us.
        if (Channel != MinecraftChannel.Brand) return;
        if (!VarInt.TryRead(Payload, out var brandBytesLength, out var bytesRead)) return;

        var brandBytes = Payload.AsSpan(bytesRead, brandBytesLength);
        var brand = Encoding.UTF8.GetString(brandBytes);
            
        if (connection.Player is null)
            throw new InvalidOperationException("Player should already be logged in at this stage.");
            
        connection.Player.Brand = brand;
    }
    
    public override void Dispose()
    {
        if (_isPayloadArrayRented)
        {
            ArrayPool<byte>.Shared.Return(Payload);
            _isPayloadArrayRented = false;
        }
        base.Dispose();
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        if (!reader.TryReadIdentifier(out var channel)) return false;
        if (reader.Remaining > PayloadMaxLength) return false;
        
        var byteArrayLength = (int)reader.Remaining;
        var payloadArray = ArrayPool<byte>.Shared.Rent(byteArrayLength);
        
        // We need to slice, because the rented array is _at least_ the requested size.
        // We only want to read up to the requested size.
        if (!reader.TryCopyTo(payloadArray.AsSpan()[..byteArrayLength]))
        {
            ArrayPool<byte>.Shared.Return(payloadArray);
            return false;
        }
        
        _isPayloadArrayRented = true;
        Channel = channel;
        Payload = payloadArray;
        return true;
    }
}