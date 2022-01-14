using Beacon.API.Net;

namespace Beacon.Server.Net;

internal interface IBeaconConnection : IConnection
{
    public Task AcceptPacketsAsync(CancellationToken cToken);
    public void ChangeState(IConnectionState state);
}
