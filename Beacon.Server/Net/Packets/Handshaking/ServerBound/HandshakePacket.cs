﻿using System.Buffers;
using Beacon.Server.Utils;

namespace Beacon.Server.Net.Packets.Handshaking.Serverbound;

public class HandshakePacket : IServerBoundPacket
{
    private bool _isRented = false;
    
    public const int PacketId = 0x00;
    public int Id => PacketId;

    public int ProtocolVersion { get; private set; }
    /// <summary>
    /// Hostname or IP, e.g. localhost or 127.0.0.1, that was used to connect.
    /// </summary>
    public string ServerAddress { get; private set; } = string.Empty;
    public ushort ServerPort { get; private set; }
    /// <summary>
    /// 1 for Status, 2 for Login.
    /// </summary>
    public ConnectionState NextState { get; private set; }
    
    public ValueTask HandleAsync(BeaconServer server, ClientConnection client)
    {
        return ValueTask.CompletedTask;
    }

    public static bool TryRentAndFill(ref SequenceReader<byte> reader, out HandshakePacket? packet)
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
            _ => throw new InvalidDataException("InvalidPacket next state in Handshake packet.")
        };

        packet = instance;
        return true;
    }

    public void Return()
    {
        if (!_isRented) return;
        _isRented = false;
        ObjectPool<HandshakePacket>.Shared.Return(this);
    }
}