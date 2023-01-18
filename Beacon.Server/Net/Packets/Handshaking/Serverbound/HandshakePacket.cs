using System.Buffers;
using Beacon.Server.Utils;

namespace Beacon.Server.Net.Packets.Handshaking.Serverbound;

public class HandshakePacket : IServerBoundPacket
{
    private bool _isRented = false;
    
    public const int PacketId = 0x00;
    public int Id => PacketId;

    public int ProtocolVersion { get; set; }
    /// <summary>
    /// Hostname or IP, e.g. localhost or 127.0.0.1, that was used to connect.
    /// </summary>
    public string ServerAddress { get; set; } = string.Empty;
    public ushort ServerPort { get; set; }
    /// <summary>
    /// 1 for Status, 2 for Login.
    /// </summary>
    public ConnectionState NextState { get; set; }
    
    public void ReadPacket(Stream stream)
    {
        ProtocolVersion = stream.ReadVarInt().value;
        ServerAddress = stream.ReadString(255);
        ServerPort = stream.ReadUnsignedShort();
        NextState = stream.ReadVarInt().value switch
        {
            1 => ConnectionState.Status,
            2 => ConnectionState.Login,
            _ => throw new InvalidDataException("Invalid next state in Handshake packet.")
        };
    }


    public ValueTask HandleAsync(BeaconServer server, ClientConnection client)
    {
        client.State = NextState;
        return ValueTask.CompletedTask;
    }

    public static bool TryRentAndFill(SequenceReader<byte> reader, out IServerBoundPacket? packet)
    {
        packet = default;

        if (!reader.TryReadVarInt(out var protocolVersion, out _))
            return false;

        if (!reader.TryReadString(out var serverAddress, out _))
            return false;

        if (!reader.TryReadUnsignedShort(out var serverPort, out _))
            return false;

        if (!reader.TryReadVarInt(out var nextState, out _))
            return false;

        var instance = ObjectPool<HandshakePacket>.Shared.Get();
        instance._isRented = true;

        instance.ProtocolVersion = protocolVersion;
        instance.ServerAddress = serverAddress;
        instance.ServerPort = serverPort;
        instance.NextState = nextState switch
        {
            1 => ConnectionState.Status,
            2 => ConnectionState.Login,
            _ => throw new InvalidDataException("Invalid next state in Handshake packet.")
        };

        packet = instance;
        return true;
    }

    public void Return()
    {
        if (_isRented) ObjectPool<HandshakePacket>.Shared.Return(this);
    }
}