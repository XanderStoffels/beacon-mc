namespace Beacon.Server.Hosting;
public class ServerOptions
{
    public string WorkingDirectory { get; set; }

	public ServerOptions()
	{
		this.WorkingDirectory = Environment.CurrentDirectory;
	}
}
