using Beacon.Server.Net;

namespace Beacon.Server.States
{
    internal interface IClientState
    {
        public ValueTask HandlePacketAsync(BeaconConnection connection, int packetId, Stream packetData, CancellationToken cToken = default);
    }
}
