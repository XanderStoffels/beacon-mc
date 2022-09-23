namespace Beacon.Server.Attributes;

[AttributeUsage(AttributeTargets.Assembly)]
internal class MincraftVersionAttribute : Attribute
{
    public MincraftVersionAttribute(string mcVersion)
    {
        MinecraftVersion = Version.Parse(mcVersion.Replace("\"", ""));
    }

    public Version MinecraftVersion { get; }
}