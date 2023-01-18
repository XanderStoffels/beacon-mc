namespace Beacon.Server.Net.Packets;

public interface IServerBoundPacket
{
    /// <summary>
    /// Replaces the values in this objects properties with data parsed from the given Span.
    /// </summary>
    /// <param name="bytes"></param>
    public void Fill(Span<byte> bytes);
    ValueTask HandleAsync(BeaconServer server, ClientConnection client);
}