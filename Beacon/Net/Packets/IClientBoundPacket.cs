namespace Beacon.Net.Packets;

public interface IClientBoundPacket
{
    public bool TryWritePayloadTo(Span<byte> buffer, out int bytesWritten);
}