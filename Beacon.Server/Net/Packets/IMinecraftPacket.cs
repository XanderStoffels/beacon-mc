namespace Beacon.Server.Net.Packets;

internal interface IMinecraftPacket
{
    ValueTask SerializeAsync(Stream stream, CancellationToken cToken = default);
    ValueTask DeserializeAsync(Stream stream, CancellationToken cToken = default);
}