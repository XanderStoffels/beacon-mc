using Microsoft.Extensions.ObjectPool;

namespace Beacon.Server.Net.Packets.Clientbound
{
    internal class ClientboundStatusPong : IPooled
    {
        public static readonly ObjectPool<ClientboundStatusPong> Pool = new DefaultObjectPool<ClientboundStatusPong>(new DefaultPooledObjectPolicy<ClientboundStatusPong>());


        public const int PACKET_ID = 0x01;
        public ulong PingId { get; set; }


        public ValueTask HydrateAsync(Stream stream, CancellationToken cToken = default)
        {
            PingId = stream.ReadLong();
            return ValueTask.CompletedTask;
        }

        public async ValueTask SerializeAsync(Stream stream, CancellationToken cToken = default)
        {
            using var memory = BeaconServer.MemoryStreamManager.GetStream("ClientboundStatusResponse");
            memory.WriteVarInt(PACKET_ID);
            memory.WriteLong(PingId);
            var bytes = memory.ToArray();

            stream.WriteVarInt(bytes.Length);
            await stream.WriteAsync(bytes);
            await stream.FlushAsync(cToken);
        }


    }
}
