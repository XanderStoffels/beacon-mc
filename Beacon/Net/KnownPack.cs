namespace Beacon.Net;

public sealed class KnownPack
{
    public string Namespace { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public static KnownPack MinecraftCore => new KnownPack
    {
        Namespace = "minecraft",
        Version = "1.21.5",
        Id = "core"
    };

}