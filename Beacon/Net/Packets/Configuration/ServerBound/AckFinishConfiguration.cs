namespace Beacon.Net.Packets.Configuration.ServerBound;

/// <summary>
/// Sent by the client to notify the server that the configuration process has finished. It is sent in response to the server's Finish Configuration.
/// </summary>
/// <remarks>
/// This class has no logic; the connection should itself handle the packet and switch to Play state.
/// </remarks>
public class AckFinishConfiguration
{
    public const int PacketId = 0x03;
}