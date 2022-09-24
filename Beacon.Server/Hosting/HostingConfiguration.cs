namespace Beacon.Server.Hosting;

public class HostingConfiguration
{
    /// <summary>
    ///     Indicates if the Host should stop running when the Beacon server stops running.
    /// </summary>
    public bool ExitOnServerShutdown { get; set; }

    internal HostingConfiguration()
    {
        ExitOnServerShutdown = true;
    }
}