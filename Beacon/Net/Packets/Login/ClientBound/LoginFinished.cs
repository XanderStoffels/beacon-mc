using Beacon.Util;

namespace Beacon.Net.Packets.Login.ClientBound;

public class LoginFinished : IClientBoundPacket
{
    public const int PacketId = 0x02;
    public Guid Uuid { get; set; }
    public string Username { get; set; } = string.Empty;
    
    public bool TryWritePayloadTo(Span<byte> buffer, out int bytesWritten)
    {
        var writer = new PayloadWriter(buffer, PacketId);
        writer.WriteUuid(Uuid);
        writer.WriteString(Username);
        writer.WriteVarInt(0); // Empty property array
        return writer.IsSuccess(out bytesWritten);
    }
}