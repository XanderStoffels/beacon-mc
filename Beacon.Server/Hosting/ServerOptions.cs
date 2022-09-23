namespace Beacon.Server.Hosting;

public class ServerOptions
{
    public ServerOptions()
    {
        WorkingDirectory = Environment.CurrentDirectory;
    }

    public string WorkingDirectory { get; set; }
}