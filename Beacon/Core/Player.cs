using System.Diagnostics;
using Beacon.Api.Enums;

namespace Beacon.Core;

[DebuggerDisplay("{Username} ({Uuid})")]
public class Player(Guid uuid, string username)
{
    public Guid Uuid { get; set; } = uuid;
    public string Username { get; set; } = username;

    public string Brand { get; set; } = string.Empty;
    
    public string Locale { get; set; } = string.Empty;
    /// <summary>
    /// Client-side render distance, in chunks.
    /// </summary>
    public sbyte ViewDistance { get; set; }
    /// <summary>
    /// See <a href="https://minecraft.wiki/w/Minecraft_Wiki:Projects/wiki.vg_merge/Chat#Client_chat_mode">Chat#Client</a> chat mode for more information.
    /// </summary>
    public ClientChatMode ChatMode { get; set; }
    /// <summary>
    /// Colors multiplayer setting. The vanilla server stores this value but does nothing with it (see MC-64867). Third-party servers such as Hypixel disable all coloring in chat and system messages when it is false.
    /// </summary>
    public bool ChatColors { get; set; }
    /// <summary>
    /// A bit mask of the skin parts that are displayed.
    /// </summary>
    public SkinParts DisplayedSkinParts { get; set; }
    public Hand MainHand { get; set; }
    /// <summary>
    /// Enables filtering of text on signs and written book titles.
    /// The vanilla client sets this according to the profanityFilterPreferences.profanityFilterOn account attribute indicated by the /player/attributes Mojang API endpoint.
    /// In offline mode it is always false.
    /// </summary>
    public bool EnableTextFiltering { get; set; }
    /// <summary>
    /// Servers usually list online players, this option should let you not show up in that list.
    /// </summary>
    public bool AllowServerListings { get; set; }
    /// <summary>
    /// Data Type: VarInt Enum <br />
    /// </summary>
    public ParticleIntensity ParticleStatus { get; set; }
}