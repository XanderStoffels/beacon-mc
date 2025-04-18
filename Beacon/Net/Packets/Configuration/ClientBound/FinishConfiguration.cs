namespace Beacon.Net.Packets.Configuration.ClientBound;

/// <summary>
/// Sent by the server to notify the client that the configuration process has finished.
/// The client answers with <see cref="Beacon.Net.Packets.Configuration.ServerBound.AckFinishConfiguration"/> whenever it is ready to continue.
/// </summary>
public class FinishConfiguration : IClientBoundPacket
{
    public const int PacketId = 0x03;
    public bool TryWritePayloadTo(Span<byte> buffer, out int bytesWritten)
    {
        bytesWritten = 0;
        return true;
    }
}