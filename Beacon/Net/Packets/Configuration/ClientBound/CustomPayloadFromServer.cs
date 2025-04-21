using Beacon.Util;

namespace Beacon.Net.Packets.Configuration.ClientBound;

/// <summary>
/// Mods and plugins can use this to send their data. Minecraft itself uses some plugin channels. These internal channels are in the minecraft namespace.
/// More documentation on this: <a href="https://dinnerbone.com/blog/2012/01/13/minecraft-plugin-channels-messaging/">dinnerbone's blogpost</a>.
/// </summary>
/// <remarks>
/// Note that the length of Data is known only from the packet length, since the packet has no length field of any kind.
/// </remarks>
public sealed class CustomPayloadFromServer : Rentable<CustomPayloadFromServer>, IClientBoundPacket
{
    public const int PacketId = 0x01;
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
    
    public bool SerializePayload(Span<byte> buffer, out int bytesWritten)
    {
        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteString(Channel);
        writer.Write(Payload);
        return writer.IsSuccess(out bytesWritten);
    }
}