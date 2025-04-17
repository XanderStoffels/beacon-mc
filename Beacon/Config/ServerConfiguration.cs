namespace Beacon.Config;

public class ServerConfiguration
{
    public const string SectionName = "Beacon";
    public required int Port { get; set; }
    
    public static ServerConfiguration Default => new()
    {
        Port = 25565
    };
}