namespace Beacon.Server;

[AttributeUsage(AttributeTargets.Assembly)]
internal class MinecraftVersionAttribute : Attribute
{
    public Version MinecraftVersion { get; }

    internal MinecraftVersionAttribute(string mcVersion)
    {
        MinecraftVersion = Version.Parse(mcVersion.Replace("\"", ""));
    }
}