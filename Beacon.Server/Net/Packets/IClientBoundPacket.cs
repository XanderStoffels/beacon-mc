namespace Beacon.Server.Net.Packets;

public interface IClientBoundPacket
{
    public Task SerializeAsync(Stream stream);
}