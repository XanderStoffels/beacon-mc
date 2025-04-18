using System.Buffers;
using Beacon.Api.Enums;
using Beacon.Core;
using Beacon.Util;

namespace Beacon.Net.Packets.Configuration.ServerBound;

/// <summary>
/// Sent when the player connects, or when settings are changed.
/// </summary>
public class ClientInformation : IServerBoundPacket, IPipeReadable<ClientInformation>, IDisposable
{
    public const int PacketId = 0x00;

    /// <summary>
    /// Data Type: String(16) <br/>
    /// e.g. en_GB.
    /// </summary>
    public string Locale { get; set; } = string.Empty;
    /// <summary>
    /// Data Type: Signed Byte <br/>
    /// Client-side render distance, in chunks.
    /// </summary>
    public sbyte ViewDistance { get; set; }
    /// <summary>
    /// Data Type: VarInt Enum <br/>
    /// See <a href="https://minecraft.wiki/w/Minecraft_Wiki:Projects/wiki.vg_merge/Chat#Client_chat_mode">Chat#Client</a> chat mode for more information.
    /// </summary>
    public ClientChatMode ChatMode { get; set; }
    /// <summary>
    /// Data Type: Boolean <br/>
    /// Colors multiplayer setting. The vanilla server stores this value but does nothing with it (see MC-64867). Third-party servers such as Hypixel disable all coloring in chat and system messages when it is false.
    /// </summary>
    public bool ChatColors { get; set; }
    /// <summary>
    /// Data Type: Unsigned Byte <br/>
    /// A bit mask of the skin parts that are displayed.
    /// </summary>
    public SkinParts DisplayedSkinParts { get; set; }
    /// <summary>
    /// Data Type: VarInt Enum <br/>
    /// </summary>
    public Hand MainHand { get; set; }
    /// <summary>
    /// Data type: Boolean <br/>
    /// Enables filtering of text on signs and written book titles.
    /// The vanilla client sets this according to the profanityFilterPreferences.profanityFilterOn account attribute indicated by the /player/attributes Mojang API endpoint.
    /// In offline mode it is always false.
    /// </summary>
    public bool EnableTextFiltering { get; set; }
    /// <summary>
    /// Data Type: Boolean <br/>
    /// Servers usually list online players, this option should let you not show up in that list.
    /// </summary>
    public bool AllowServerListings { get; set; }
    /// <summary>
    /// Data Type: VarInt Enum <br />
    /// </summary>
    public ParticleIntensity ParticleStatus { get; set; }
    
    private bool _isThisRented;
    
    public void Handle(Server server, Connection connection)
    {
        // Nothing to do for now, later update the player info.
        if (connection.Player is null)
            throw new InvalidOperationException("Player should already be logged in at this stage.");
        
        ApplyTo(connection.Player);
        Console.WriteLine($"" +
                          $"Locale: {Locale}, " +
                          $"ViewDistance: {ViewDistance}, " +
                          $"ChatMode: {ChatMode}, " +
                          $"ChatColors: {ChatColors}, " +
                          $"DisplayedSkinParts: {DisplayedSkinParts}, " +
                          $"MainHand: {MainHand}, " +
                          $"EnableTextFiltering: {EnableTextFiltering}, " +
                          $"AllowServerListings: {AllowServerListings}, " +
                          $"ParticleStatus: {ParticleStatus}");
    }

    public void ApplyTo(Player player)
    {
        player.Locale = Locale;
        player.ViewDistance = ViewDistance;
        player.ChatMode = ChatMode;
        player.ChatColors = ChatColors;
        player.DisplayedSkinParts = DisplayedSkinParts;
        player.MainHand = MainHand;
        player.EnableTextFiltering = EnableTextFiltering;
        player.AllowServerListings = AllowServerListings;
        player.ParticleStatus = ParticleStatus;
    }
    
    public void Dispose()
    {
        if (!_isThisRented) return;
        ObjectPool<ClientInformation>.Shared.Return(this);
        _isThisRented = false;
    }

    public static ClientInformation Deserialize(ref SequenceReader<byte> reader)
    {
        reader.TryReadString(out var locale);
        
        reader.TryRead(out var viewDistanceByte);
        var viewDistance = Convert.ToSByte(viewDistanceByte);
        
        reader.TryReadVarInt(out var chatModeVarInt);
        var chatMode = (ClientChatMode)chatModeVarInt;
        
        reader.TryRead(out var chatColorsByte);
        var chatColors = Convert.ToBoolean(chatColorsByte);
        
        reader.TryRead(out var displayedSkinPartsByte);
        var displayedSkinParts = (SkinParts)displayedSkinPartsByte;
        
        reader.TryReadVarInt(out var mainHandVarInt);
        var mainHand = (Hand)mainHandVarInt;
        
        reader.TryRead(out var enableTextFilteringByte);
        var enableTextFiltering = Convert.ToBoolean(enableTextFilteringByte);
        
        reader.TryRead(out var allowServerListingsByte);
        var allowServerListings = Convert.ToBoolean(allowServerListingsByte);
        
        reader.TryReadVarInt(out var particleStatusVarInt);
        var particleStatus = (ParticleIntensity)particleStatusVarInt;
        
        var packet = ObjectPool<ClientInformation>.Shared.Get();
        packet._isThisRented = true;
        packet.Locale = locale;
        packet.ViewDistance = viewDistance;
        packet.ChatMode = chatMode;
        packet.ChatColors = chatColors;
        packet.DisplayedSkinParts = displayedSkinParts;
        packet.MainHand = mainHand;
        packet.EnableTextFiltering = enableTextFiltering;
        packet.AllowServerListings = allowServerListings;
        packet.ParticleStatus = particleStatus;
        return packet;
    }
}