namespace Beacon.Server.Net
{
    internal interface IConnectionState
    {
        public ValueTask HandlePacketAsync(int packetId, Stream packetData, CancellationToken cToken = default);
    }
}
