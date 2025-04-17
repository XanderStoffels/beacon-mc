using System.Buffers;
using Beacon.Util;

namespace Beacon.Net.Packets.Handshaking.ServerBound;

public class Handshake : IServerBoundPacket, IPipeReadable<Handshake>, IDisposable
{
    public const int PacketId = 0x00;
    public int ProtocolVersion { get; private set; }
    public string ServerAddress { get; private set; } = string.Empty;
    public ushort ServerPort { get; private set; }
    public int NextState { get; private set; }
    private bool _isRented;
    public void Handle(Server server, Connection connection)
    {
        // Nothing to do here.
    }

    public void Dispose()
    {
        if (!_isRented) return;
        ObjectPool<Handshake>.Shared.Return(this);
        _isRented = false;
    }
    
    public static Handshake Deserialize(ref SequenceReader<byte> reader)
    {
        reader.TryReadVarInt(out var protocolVersion, out _);
        reader.TryReadString(out var serverAddress);
        reader.TryReadUShort(out var serverPort);
        reader.TryReadVarInt(out var nextState, out _);
        
        var handshake = ObjectPool<Handshake>.Shared.Get();
        handshake.ProtocolVersion = protocolVersion;    
        handshake.ServerAddress = serverAddress;
        handshake.ServerPort = serverPort;
        handshake.NextState = nextState;
        handshake._isRented = true;
        return handshake;
    }
}