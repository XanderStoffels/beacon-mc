using System.Buffers;
using Beacon.Net.Packets.Login.ClientBound;
using Beacon.Util;

namespace Beacon.Net.Packets.Login.ServerBound;

/// <summary>
/// Login Start.
/// </summary>
public class Hello : IServerBoundPacket, IPipeReadable<Hello>, IDisposable
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
    private bool _isRented;
    
    public void Handle(Server server, Connection connection)
    {
        server.EnqueuePacket(new LoginFinished
        {
            Username = Name,
            Uuid = PlayerUuid
        }, connection);
    }

    public static Hello Deserialize(ref SequenceReader<byte> reader)
    {
        reader.TryReadString(out var username);
        reader.TryReadUuid(out var playerUuid);
        
        var hello = ObjectPool<Hello>.Shared.Get();
        hello._isRented = true;
        hello.Name = username;
        hello.PlayerUuid = playerUuid;
        
        return new Hello
        {
            Name = username,
            PlayerUuid = playerUuid
        };
    }

    public void Dispose()
    {
        if (!_isRented) return;
        ObjectPool<Hello>.Shared.Return(this);
        _isRented = false;
    }
}