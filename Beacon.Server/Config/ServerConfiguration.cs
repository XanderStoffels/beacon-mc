namespace Beacon.Server.Config
{
    internal class ServerConfiguration
    {
        public int Port { get; set; }

        public static ServerConfiguration Default => new()
        {
            Port = 25565
        };
    }
}
