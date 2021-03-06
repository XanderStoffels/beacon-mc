using Beacon.Server.Net;
using Microsoft.Extensions.Logging;
using Beacon.API.Events;
using Beacon.Server.Net.Packets;
using Beacon.Server.Utils;

namespace Beacon.Server.States
{
    internal class StatusState : IConnectionState
    {
        private readonly IMinecraftEventBus _eventBus;
        private readonly ILogger _logger;
        private readonly IBeaconConnection _connection;
        private bool _hasReceivedPacket;
        public StatusState(IBeaconConnection connection)
        {
            _hasReceivedPacket = false;
            _connection = connection;
            _logger = connection.Server.Logger;
            _eventBus = connection.Server.EventBus; 
        }

        public async ValueTask HandlePacketAsync(int packetId, Stream packetData, CancellationToken cToken = default)
        {
            switch (packetId)
            {
                case 0x00 when !_hasReceivedPacket:
                    await HandleStatusRequest(_connection, cToken);
                    break;

                case 0x00 when _hasReceivedPacket:
                    _logger.LogDebug("{ip} has sent a packet that is not valid! Terminating connection", _connection.RemoteAddress);
                    _connection.Dispose();
                    break;

                case 0x01:
                    await HandlePing(_connection, packetData, cToken);
                    break;
            }
        }


        private static async ValueTask HandlePing(IBeaconConnection connection, Stream packetData, CancellationToken cToken)
        {
            var pong = ObjectPool<ClientboundStatusPong>.Shared.Get();
            await pong.DeserializeAsync(packetData, cToken);
            await pong.SerializeAsync(connection.Stream, cToken);
            ObjectPool<ClientboundStatusPong>.Shared.Return(pong);
        }

        private async ValueTask HandleStatusRequest(IBeaconConnection connection, CancellationToken cToken)
        {
            var status = await connection.Server.GetStatusAsync();
            var e = new ServerStatusRequestEvent(connection.Server, connection.RemoteAddress, status);
            await _eventBus.FireEventAsync(e, cToken);

            if (e.IsCancelled) return;
            
            var packet = ObjectPool<ClientboundStatusResponse>.Shared.Get();
            packet.ServerStatus = status;
            await packet.SerializeAsync(connection.Stream, cToken);
            _hasReceivedPacket = true;
            ObjectPool<ClientboundStatusResponse>.Shared.Return(packet);
        }
    }
}
