namespace Beacon.Net.Packets;

public interface IServerBoundPacket
{
    /// <summary>
    /// Handle the incoming packet.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="connection">The connection that sent this packet to the server.</param>
    public void Handle(Server server, Connection connection);
}