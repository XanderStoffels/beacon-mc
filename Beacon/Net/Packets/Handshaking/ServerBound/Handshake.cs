using System.Buffers;
using Beacon.Util;

namespace Beacon.Net.Packets.Handshaking.ServerBound;

public class Handshake : Rentable<Handshake>, IServerBoundPacket
{
    public const int PacketId = 0x00;
    public int ProtocolVersion { get; private set; }
    public string ServerAddress { get; private set; } = string.Empty;
    public ushort ServerPort { get; private set; }
    public int NextState { get; private set; }
    
    public void Handle(Server server, Connection connection)
    {
        // Nothing to do here.
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        reader.TryReadVarInt(out var protocolVersion, out _);
        reader.TryReadString(out var serverAddress);
        reader.TryReadUShort(out var serverPort);
        reader.TryReadVarInt(out var nextState, out _);
        
        ProtocolVersion = protocolVersion;    
        ServerAddress = serverAddress;
        ServerPort = serverPort;
        NextState = nextState;
        return true;
    }
    
}