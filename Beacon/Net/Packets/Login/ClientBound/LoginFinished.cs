using Beacon.Util;

namespace Beacon.Net.Packets.Login.ClientBound;

/// <summary>
/// Indicates that the login process was finished successfully.
/// </summary>
public class LoginFinished : Rentable<LoginFinished>, IClientBoundPacket
{
    public const int PacketId = 0x02;
    public Guid Uuid { get; set; }
    public string Username { get; set; } = string.Empty;
    
    // TODO: This packet is unfinished. It is missing some properties not used in offline mode.
    
    public bool SerializePayload(Span<byte> buffer, out int bytesWritten)
    {
        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteUuid(Uuid);
        writer.WriteString(Username);
        writer.WriteVarInt(0); // Empty property array
        return writer.IsSuccess(out bytesWritten);
    }
}