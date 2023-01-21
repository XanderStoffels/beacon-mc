using System.Buffers;
using Beacon.Server.Net.Packets.Status.ClientBound;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Net.Packets.Status.ServerBound;

public class StatusRequestPacket : IServerBoundPacket
{    
    private bool _isRented;
    public int Id => 0x00;
    
    public async ValueTask HandleAsync(BeaconServer server, ClientConnection client)
    {
        var packet = ObjectPool<StatusResponsePacket>.Shared.Get();
        packet.ServerStatus = server.Status;
        server.Logger.LogDebug("[{IP}] Sending server status", client.Ip);
        await packet.SerializeAsync(client.NetworkStream);
    }

    public static bool TryRentAndFill(ref SequenceReader<byte> reader, out IServerBoundPacket? packet)
    {
        var instance = ObjectPool<StatusRequestPacket>.Shared.Get();
        instance._isRented = true;
        packet = instance;
        return true;
    }

    public void Return()
    {
        if (!_isRented) return;
        _isRented = false;
        ObjectPool<StatusRequestPacket>.Shared.Return(this);
    }
}