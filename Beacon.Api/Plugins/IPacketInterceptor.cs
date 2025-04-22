using Beacon.Api.Net;

namespace Beacon.Api.Plugins;

public interface IPacketInterceptor
{
    /// <summary>
    /// Used to identify the type of packet to intercept.
    /// </summary>
    public PacketIdentifier Key { get; }
    /// <summary>
    /// Intercept a packet with its data in the given buffer before it is handled by the server.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="connection"></param>
    /// <param name="buffer"></param>
    /// <returns>True if the server should ignore the packet, false otherwise.</returns>
    public bool Intercept(IServer server, IConnection connection, ReadOnlySpan<byte> buffer);
}

/// <summary>
/// A key that uniquely identifies a type of packet in the minecraft protocol.
/// </summary>
/// <param name="ConnectionState"></param>
/// <param name="PacketId"></param>
public readonly record struct PacketIdentifier(int ConnectionState, int PacketId);