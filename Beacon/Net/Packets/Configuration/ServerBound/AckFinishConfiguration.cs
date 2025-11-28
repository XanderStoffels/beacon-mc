using System.Buffers;
using Beacon.Api.Enums;

namespace Beacon.Net.Packets.Configuration.ServerBound;

/// <summary>
/// Sent by the client to notify the server that the configuration process has finished. It is sent in response to the server's Finish Configuration.
/// </summary>
public sealed class AckFinishConfiguration : IServerBoundPacket
{
    public const int PacketId = 0x03;
    public static AckFinishConfiguration Instance => _instance ??= new AckFinishConfiguration();
    private static AckFinishConfiguration? _instance;
    private AckFinishConfiguration() {}
    
    
    public void Handle(Server server, Connection connection)
    {
        // The client is ready to play. Send all necessary packets.
        
        // Play (login) (id ox2B)
        var loginPacket = Play.ClientBound.Login.Rent();
        loginPacket.EntityId = 0;
        loginPacket.DimensionNames.Clear();
        loginPacket.DimensionNames.Add("overworld");
        loginPacket.ReducedDebugInfo = false;
        loginPacket.DimensionName = "overworld";
        loginPacket.DimensionType = 0;
        loginPacket.HashedSeed = 0;
        loginPacket.GameMode = GameMode.Creative;
        loginPacket.PreviousGameMode = null;
        
        connection.EnqueuePacket(loginPacket);

        // SetDefaultSpawnPosition (id 0x5A)
        // SetTime (id 0x6A)
        // GameEvent SetWeather (id 0x22) 
        // SetDifficulty (id 0x0A)
        // Brand as a custom payload (id 0x18)
        // Commands (id 0x10)
    }

    public bool DeserializePayload(ref SequenceReader<byte> reader)
    {
        return true;
    }
}