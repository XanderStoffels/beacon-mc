using System.Buffers;
using Beacon.Server.Net.Packets.Status.ClientBound;
using Beacon.Server.Utils;

namespace Beacon.Server.Net.Packets.Status.ServerBound;

public class PingRequestPacket : IServerBoundPacket
{
    private const int PacketId = 0x01;
    private bool _isRented;
    public int Id => PacketId;
    public long Payload { get; private set; }
    
    public async ValueTask HandleAsync(BeaconServer server, ClientConnection client)
    {
        var response = ObjectPool<PingResponsePacket>.Shared.Get();
        response.Payload = Payload;
        await response.SerializeAsync(client.NetworkStream);
        ObjectPool<PingResponsePacket>.Shared.Return(response);
    }
    
    
    public static bool TryRentAndFill(ref SequenceReader<byte> reader, out PingRequestPacket? packet)
    {
        packet = default;
        if (!reader.TryReadBigEndian(out long payload))
            return false;
        
        var instance = ObjectPool<PingRequestPacket>.Shared.Get();
        instance._isRented = true;
        instance.Payload = payload;
        packet = instance;
        return true;
    }

    public void Return()
    {
        if (!_isRented) return;
        _isRented = false;
        ObjectPool<PingRequestPacket>.Shared.Return(this);
    }
}