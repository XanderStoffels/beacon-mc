using Beacon.API.Events;
using Beacon.API.Net;
using Beacon.Server.Net;
using Beacon.Server.Net.Packets;
using Beacon.Server.Utils;

namespace Beacon.Server.States;

internal class StatusState : IConnectionState
{
    private readonly IBeaconConnection _connection;
    private readonly IMinecraftEventBus _eventBus;
    private bool _hasReceivedPacket;

    public StatusState(IBeaconConnection connection)
    {
        _hasReceivedPacket = false;
        _connection = connection;
        _eventBus = connection.Server.Events;
    }

    public async ValueTask HandlePacketAsync(int packetId, Stream packetData, CancellationToken cToken = default)
    {
        switch (packetId)
        {
            case 0x00 when !_hasReceivedPacket:
                await HandleStatusRequest(_connection, cToken);
                break;

            case 0x00 when _hasReceivedPacket:
                _connection.Dispose();
                break;

            case 0x01:
                await HandlePing(_connection, packetData, cToken);
                break;
        }
    }


    private static async ValueTask HandlePing(IConnection connection, Stream packetData, CancellationToken cToken)
    {
        var pong = ObjectPool<ClientboundStatusPong>.Shared.Get();
        await pong.DeserializeAsync(packetData, cToken);
        await pong.SerializeAsync(connection.Stream, cToken);
        ObjectPool<ClientboundStatusPong>.Shared.Return(pong);
    }

    private async ValueTask HandleStatusRequest(IConnection connection, CancellationToken cToken)
    {
        var status = connection.Server.GetStatus();
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