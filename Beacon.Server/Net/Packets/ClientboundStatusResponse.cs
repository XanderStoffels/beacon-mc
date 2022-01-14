using Beacon.API.Models;
using Beacon.Server.Utils;
using System.Text.Json;

namespace Beacon.Server.Net.Packets
{
    internal class ClientboundStatusResponse : IMinecraftPacket
    {
        public const int PACKET_ID = 0x00;
        public ServerStatus? ServerStatus { get; set; }

        public ValueTask DeserializeAsync(Stream stream, CancellationToken cToken = default)
        {
            return ValueTask.CompletedTask;
        }


        public async ValueTask SerializeAsync(Stream stream, CancellationToken cToken = default)
        {
            if (ServerStatus == null)
                ServerStatus = new ServerStatus();

            var json = JsonSerializer.Serialize(ServerStatus,
               new JsonSerializerOptions
               {
                   PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                   DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
               });

            using var memory = MemoryStreaming.Manager.GetStream("ClientboundStatusResponse");
            memory.WriteVarInt(PACKET_ID);
            memory.WriteString(json);
            var bytes = memory.ToArray();

            stream.WriteVarInt(bytes.Length);
            await stream.WriteAsync(bytes, cToken);
            await stream.FlushAsync(cToken);
        }

    }
}
