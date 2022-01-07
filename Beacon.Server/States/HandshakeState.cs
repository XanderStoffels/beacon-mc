using Beacon.Server.Net;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Server.States
{
    internal class HandshakeState : IClientState
    {
        private readonly IServiceProvider _provider;
        public HandshakeState(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async ValueTask HandlePacketAsync(BeaconConnection connection, int packetId, Stream packetData, CancellationToken cToken)
        {
            if (packetId != 0x00)
            {
                connection.Close();
                return;
            }

            var protocolVersion = (await packetData.ReadVarIntAsync()).value;
            var serverAddress = packetData.ReadString(255);
            var port = packetData.ReadUnsignedShort();
            var nextState = (await packetData.ReadVarIntAsync()).value;

            if (nextState == 1)
            {
                var statusState = _provider.GetRequiredService<StatusState>();
                connection.ChangeState(statusState);
            }
            else if (nextState == 2)
            {

            }
            else
            {

            }

            return;
        }

    }
}
