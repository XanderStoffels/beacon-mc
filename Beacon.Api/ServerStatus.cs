using System.Text.Json.Serialization;

namespace Beacon.Api;

public class ServerStatus(
    string versionName, int versionProtocol,
    int maxPlayerCount, int playerCount,
    string description,
    string favicon,
    bool enforcesSecureChat,
    params ServerStatus.StatusPlayerSample[] playerSamples)
{
    [JsonPropertyName("version")]
    public StatusVersion Version { get; } = new(versionName, versionProtocol);

    [JsonPropertyName("players")]
    public StatusPlayers Players { get; } = new(maxPlayerCount, playerCount, playerSamples);

    [JsonPropertyName("description")]
    public StatusDescription Description { get; } = new(description);

    [JsonPropertyName("favicon")]
    public string Favicon { get; } = favicon;

    [JsonPropertyName("enforcesSecureChat")]
    public bool EnforcesSecureChat { get; } = enforcesSecureChat;

    public class StatusVersion
    {
        [JsonPropertyName("name")]
        public string Name { get; }
    
        [JsonPropertyName("protocol")]
        public int Protocol { get; }
    
        public StatusVersion(string name, int protocol)
        {
            Name = name;
            Protocol = protocol;
        }
    }
    
    public class StatusPlayers(int max, int online, params StatusPlayerSample[] sample)
    {
        [JsonPropertyName("max")]
        public int Max { get; } = max;

        [JsonPropertyName("online")]
        public int Online { get; } = online;

        [JsonPropertyName("sample")]
        public StatusPlayerSample[] Sample { get; } = sample;
    }
    
    public class StatusPlayerSample(string name, string id)
    {
        [JsonPropertyName("name")]
        public string Name { get; } = name;

        [JsonPropertyName("id")]
        public string Id { get; } = id;
    }
    
    public class StatusDescription
    {
        [JsonPropertyName("text")]
        public string Text { get; }
    
        public StatusDescription(string text)
        {
            Text = text;
        }
    }
}