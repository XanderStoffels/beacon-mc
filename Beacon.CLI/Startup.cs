using Beacon.Hosting;
using Beacon.Server;

internal class Startup : IBeaconStartup
{
    public Task<ServerConfiguration> LoadConfigurationAsync()
    {
        return Task.FromResult(new ServerConfiguration
        {
            Port = 25565,
        });
    }

    public async IAsyncEnumerable<string> ProvideConsoleCommands(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            var input = Console.ReadLine();
            if (input == null) continue;
            yield return input;
        }
    }
}