using Beacon.Server;

namespace Beacon.Hosting;

public interface IBeaconStartup
{
    Task<ServerConfiguration> LoadConfigurationAsync();
    IAsyncEnumerable<string> ProvideConsoleCommands(CancellationToken cancelToken);
}