namespace Beacon.Server
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    internal class MincraftVersionAttribute : Attribute
    {
        public Version MinecraftVersion { get; }
        public MincraftVersionAttribute(string mcVersion)
        {
            this.MinecraftVersion = Version.Parse(mcVersion.Replace("\"", ""));
        }
    }
}
