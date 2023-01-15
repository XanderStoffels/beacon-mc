namespace Beacon.Server.Net.Packets.Handshaking.Serverbound;

public class HandshakePacket : IServerBoundPacket
{
    public const int PacketId = 0x00;
    
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

    public async ValueTask ReadPacketAsync(Stream stream)
    {
        ProtocolVersion = (await stream.ReadVarIntAsync()).value;
        ServerAddress = await stream.ReadStringAsync(255);
        ServerPort = await stream.ReadUnsignedShortAsync();
        NextState = (await stream.ReadVarIntAsync()).value switch
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
}