using Beacon.Server.Net;
using Beacon.Server.Net.Packets.Clientbound;
using Microsoft.Extensions.Logging;
using Beacon.API.Events;
using Beacon.Server.Plugins.Events;

namespace Beacon.Server.States
{
    internal class StatusState : IClientState
    {
        private readonly ILogger<StatusState> _logger;
        private readonly IMinecraftEventBus _eventBus;
        private bool _hasReceivedPacket = false;

        public StatusState(ILogger<StatusState> logger, IMinecraftEventBus plugins)
        {
            _logger = logger;
            _eventBus = plugins;
        }

        public async ValueTask HandlePacketAsync(BeaconConnection connection, int packetId, Stream packetData, CancellationToken cToken)
        {
            switch (packetId)
            {
                case 0x00 when !_hasReceivedPacket:
                    await HandleStatusRequest(connection, cToken);
                    break;

                case 0x00 when _hasReceivedPacket:
                    _logger.LogDebug("{ip} has sent a packet that is not valid! Terminating connection", connection.Tcp.Client.RemoteEndPoint?.ToString() ?? "Unknown");
                    connection.Close();
                    break;

                case 0x01:
                    await HandlePing(connection, packetData, cToken);
                    break;
            }
        }

        private static async ValueTask HandlePing(BeaconConnection connection, Stream packetData, CancellationToken cToken)
        {
            var pong = ClientboundStatusPong.Pool.Get();
            await pong.HydrateAsync(packetData, cToken);
            await pong.SerializeAsync(connection.GetStream(), cToken);
            ClientboundStatusPong.Pool.Return(pong);
        }

        private async ValueTask HandleStatusRequest(BeaconConnection connection, CancellationToken cToken)
        {
            var status = await connection.Server.GetStatusAsync();
            var e = new ServerStatusRequestedEvent(connection.Tcp.Client.RemoteEndPoint, status);
            await _eventBus.FireEventAsync(e, cToken);

            if (e.IsCancelled) return;
            
            var packet = ClientboundStatusResponse.Pool.Get();
            packet.ServerStatus = status;
            await packet.SerializeAsync(connection.GetStream(), cToken);
            ClientboundStatusResponse.Pool.Return(packet);
            _hasReceivedPacket = true;

        }
    }
}
