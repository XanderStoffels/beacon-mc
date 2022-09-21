namespace Beacon.Server.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    internal class MincraftVersionAttribute : Attribute
    {
        public Version MinecraftVersion { get; }
        public MincraftVersionAttribute(string mcVersion)
        {
            MinecraftVersion = Version.Parse(mcVersion.Replace("\"", ""));
        }
    }
}
