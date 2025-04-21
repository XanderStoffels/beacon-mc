using System.Buffers;
using Beacon.Core;
using Beacon.Net.Packets.Login.ClientBound;
using Beacon.Util;

namespace Beacon.Net.Packets.Login.ServerBound;

/// <summary>
/// Login Start.
/// </summary>
public class Hello : Rentable<Hello>, IServerBoundPacket
{
    public const int PacketId = 0x00;

    /// <summary>
    /// Data Type: String(16) <br/>
    /// The player's username.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Data Type: UUID
    /// The UUID of the player logging in. Unused by the vanilla server.
    /// </summary>
    public Guid PlayerUuid { get; set; } = Guid.Empty;
    
    public void Handle(Server server, Connection connection)
    {
        connection.Player = new Player(PlayerUuid, Name);
        
        var responsePacket = LoginFinished.Rent();
        responsePacket.Username = Name;
        responsePacket.Uuid = PlayerUuid;
        connection.EnqueuePacket(responsePacket);
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        reader.TryReadString(out var username);
        reader.TryReadUuid(out var playerUuid);
        
        Name = username;
        PlayerUuid = playerUuid;
        return true;
    }
    
}