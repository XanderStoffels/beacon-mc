using Beacon.Server.Net;
using Beacon.Server.Net.Packets.Clientbound;
using Beacon.Server.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Beacon.API.Events;
using Beacon.API;

namespace Beacon.Server.States
{
    internal class StatusState : IClientState
    {
        private readonly ILogger<StatusState> _logger;
        private readonly IPluginController _plugins;
        private bool _hasReceivedPacket = false;

        public StatusState(ILogger<StatusState> logger, IPluginController plugins)
        {
            _logger = logger;
            _plugins = plugins;
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
            await _plugins.FireEventAsync(e);

            if (e.IsCancelled) return;
            
            var packet = ClientboundStatusResponse.Pool.Get();
            packet.ServerStatus = status;
            await packet.SerializeAsync(connection.GetStream(), cToken);
            ClientboundStatusResponse.Pool.Return(packet);
            _hasReceivedPacket = true;

        }
    }
}
