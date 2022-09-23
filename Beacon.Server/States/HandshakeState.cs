using Beacon.Server.Net;

namespace Beacon.Server.States;

internal class HandshakeState : IConnectionState
{
    private readonly IBeaconConnection _connection;

    public HandshakeState(IBeaconConnection connection)
    {
        _connection = connection;
    }

    public async ValueTask HandlePacketAsync(int packetId, Stream packetData, CancellationToken cToken = default)
    {
        if (packetId != 0x00)
        {
            _connection.Dispose();
            return;
        }

        var protocolVersion = (await packetData.ReadVarIntAsync()).value;
        var serverAddress = packetData.ReadString(255);
        var port = packetData.ReadUnsignedShort();
        var nextState = (await packetData.ReadVarIntAsync()).value;

        if (nextState == 1)
        {
            var statusState = new StatusState(_connection);
            _connection.ChangeState(statusState);
        }
        else if (nextState == 2)
        {
        }
    }
}