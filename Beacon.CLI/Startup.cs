using System.Runtime.CompilerServices;
using Beacon.Hosting;
using Beacon.Server;

namespace Beacon.CLI;

internal class Startup : IBeaconStartup
{
    public Task<ServerConfiguration> LoadConfigurationAsync()
    {
        return Task.FromResult(new ServerConfiguration
        {
            Port = 25565,
        });
    }

    public async IAsyncEnumerable<string> ProvideConsoleCommands([EnumeratorCancellation] CancellationToken cancelToken)
    {
        // Read the Console asynchronously.
        while (!cancelToken.IsCancellationRequested)
        {
            var line = await Console.In.ReadLineAsync(cancelToken);
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            yield return line;
        }
    }
}