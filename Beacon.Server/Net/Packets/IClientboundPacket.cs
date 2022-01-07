namespace Beacon.Server.Net.Packets
{
    internal interface IClientboundPacket : IPooled
    {
        ValueTask SerializeAsync(Stream stream, CancellationToken cToken = default);
    }
}
