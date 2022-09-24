using Beacon.Server.Config;

namespace Beacon.Server.Hosting;

public class ServerEnvironment
{
    public HostingConfiguration HostingConfiguration { get; }

    public ServerConfiguration ServerConfiguration { get; }

    /// <summary>
    ///     The directory where the server will look for all its files.
    ///     Files include worlds, configs, etc.
    /// </summary>
    public DirectoryInfo WorkingDirectory { get; set; }

    /// <summary>
    ///     s
    ///     A text reader for server commands to be read from.
    ///     Defaults to <see cref="Console.In" />.
    /// </summary>
    public TextReader InputStream { get; set; }

    /// <summary>
    ///     s
    ///     A text writer for server output to be written to.
    ///     Defaults to <see cref="Console.Out" />.
    /// </summary>
    public TextWriter OutputStream { get; set; }

    internal ServerEnvironment()
    {
        ServerConfiguration = new ServerConfiguration();
        HostingConfiguration = new HostingConfiguration();
        WorkingDirectory = new DirectoryInfo(Environment.CurrentDirectory);
        InputStream = Console.In;
        OutputStream = Console.Out;
    }
}