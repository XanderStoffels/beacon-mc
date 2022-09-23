using Beacon.Server.Utils;

namespace Beacon.Server.Net.Packets;

internal class ClientboundStatusPong : IMinecraftPacket
{
    public const int PACKET_ID = 0x01;
    public ulong PingId { get; set; }


    public async ValueTask SerializeAsync(Stream stream, CancellationToken cToken = default)
    {
        using var memory = MemoryStreaming.Manager.GetStream("ClientboundStatusResponse");
        memory.WriteVarInt(PACKET_ID);
        memory.WriteLong(PingId);
        var bytes = memory.ToArray();

        stream.WriteVarInt(bytes.Length);
        await stream.WriteAsync(bytes, cToken);
        await stream.FlushAsync(cToken);
    }

    public ValueTask DeserializeAsync(Stream stream, CancellationToken cToken = default)
    {
        PingId = stream.ReadLong();
        return ValueTask.CompletedTask;
    }
}