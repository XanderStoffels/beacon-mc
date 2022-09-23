namespace Beacon.Server.Hosting;

public class BeaconHostingOptions
{
    internal BeaconHostingOptions()
    {
        ServerOptions = new ServerOptions();
    }

    public ServerOptions ServerOptions { get; }
}