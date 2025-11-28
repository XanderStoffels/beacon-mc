using Beacon.Api;
using Beacon.Api.Enums;
using Beacon.Util;

namespace Beacon.Net.Packets.Play.ClientBound;

public class Login : Rentable<Login>, IClientBoundPacket
{
    public const int PacketId = 0x2B;
    
    /// <summary>
    /// Data Type: Int <br/>
    /// The player's entity ID.
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// </summary>
    public bool IsHardcore { get; set; }
    
    /// <summary>
    /// Data Type: Prefixed Array of Identifier
    /// </summary>
    public List<string> DimensionNames { get; set; } = [];
    
    /// <summary>
    /// Data Type: VarInt <br/>
    /// Was once used by the client to draw the player list, but is now ignored. 
    /// </summary>
    public int MaxPlayers { get; set; } = 10;
    
    /// <summary>
    /// Data Type: VarInt <br/>
    /// Render distance in chunks (2-32)
    /// </summary>
    public int ViewDistance { get; set; } = 8;
    
    /// <summary>
    /// Data Type: VarInt
    /// The distance that the client will process specific things, such as entities.
    /// </summary>
    public int SimulationDistance { get; set; } = 12;
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// If true, a vanilla client shows reduced information on the debug screen.
    /// For servers in development, this should almost always be false.
    /// </summary>
    public bool ReducedDebugInfo { get; set; } = false;
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// Set to false when the doImmediateRespawn gamerule is true.
    /// </summary>
    public bool EnableRespawnScreen { get; set; } = true;
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// Whether players can only craft recipes they have already unlocked. Currently unused by the client.
    /// </summary>
    public bool DoLimitedCrafting { get; set; } = false;
    
    /// <summary>
    /// Data Type: VarInt <br/>
    /// The ID of the type of dimension in the minecraft:dimension_type registry, defined by the Registry Data packet.
    /// </summary>
    public int DimensionType { get; set; }
    
    /// <summary>
    /// Data Type: Identifier <br/>
    /// Name of the dimension beeing spawned into.
    /// </summary>
    public string DimensionName { get; set; } = string.Empty;
    
    /// <summary>
    /// Data Type: Long <br/>
    /// First 8 bytes of the hashed seed of the world. Used client side for biome noise.
    /// </summary>
    public long HashedSeed { get; set; }
    
    /// <summary>
    /// Data Type: Unsigned Byte <br/>
    /// </summary>
    public GameMode GameMode { get; set; } = GameMode.Survival;
    
    /// <summary>
    /// Data Type: Signed Byte <br/>
    /// The previous game mode. Vanilla client uses this for the debug (F3 + N & F3 + F4) game mode switch.
    /// Serialized to -1 if null.
    /// </summary>
    public GameMode? PreviousGameMode { get; set; } = null;
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// True if the world is a debug mode world; debug mode worlds cannot be modified and have predefined blocks.
    /// </summary>
    public bool IsDebug { get; set; } = false;
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// True if the world is a superflat world; flat worlds have different void fog and a horizon at y=0 instead of y=63.
    /// </summary>
    public bool IsFlat { get; set; } = false;
    
    /// <summary>
    /// Data Type: Boolean <br/>
    /// If true, then the next two fields are present.
    /// </summary>
    public bool HasDeathLocation { get; set; } = false;
    
    /// <summary>
    /// Data Type: Optional Identifier <br/>
    /// Name of the dimension the player died in.
    /// </summary>
    public string? DeathDimension { get; set; } = null;
    
    /// <summary>
    /// Data Type: Optional Position <br/>
    /// The location that the player died at.
    /// </summary>
    public Position? DeathLocation { get; set; } = null;
    
    /// <summary>
    /// Data Type: VarInt <br/>
    /// The number of ticks until the player can use the portal again.
    /// </summary>
    public int PortalCooldown { get; set; }

    /// <summary>
    /// Data Type: VarInt <br/>
    /// </summary>
    public int SeaLevel { get; set; } = 64;

    /// <summary>
    /// Data Type: Boolean <br/>
    /// </summary>
    public bool EnforcesSecureChat { get; set; } = false;
        
    
    
    public bool SerializePayload(Span<byte> buffer, out int bytesWritten)
    {
        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteInt(EntityId);
        writer.WriteBool(IsHardcore);
        writer.WritePrefixedIdentifierArray(DimensionNames);
        writer.WriteVarInt(MaxPlayers);
        writer.WriteVarInt(ViewDistance);
        writer.WriteVarInt(SimulationDistance);
        writer.WriteBool(ReducedDebugInfo);
        writer.WriteBool(EnableRespawnScreen);
        writer.WriteBool(DoLimitedCrafting);
        writer.WriteVarInt(DimensionType);
        writer.WriteIdentifier(DimensionName);
        writer.WriteLong(HashedSeed);
        writer.WriteUnsignedByte((byte)GameMode);
        
        if (!PreviousGameMode.HasValue)
            writer.WriteSignedByte(-1);
        else
            writer.WriteSignedByte((sbyte)PreviousGameMode.Value);
        
        writer.WriteBool(IsDebug);
        writer.WriteBool(IsFlat);
        writer.WriteBool(HasDeathLocation);
        
        if (HasDeathLocation)
        {
            if (DeathDimension is null)
                throw new InvalidOperationException("DeathDimension is null while HasDeathLocation is true");
            
            if (!DeathLocation.HasValue)
                throw new InvalidOperationException("DeathLocation is null while HasDeathLocation is true");
            
            writer.WriteIdentifier(DeathDimension);
            writer.WritePosition(DeathLocation.Value);
        }
        
        writer.WriteVarInt(PortalCooldown);
        writer.WriteVarInt(SeaLevel);
        writer.WriteBool(EnforcesSecureChat);
        return writer.IsSuccess(out bytesWritten);
    }
}