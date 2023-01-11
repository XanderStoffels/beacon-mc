using Beacon.API;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public class BeaconServer : IServer
{
    private readonly ILogger _logger;
    private readonly LoggerFactory _loggerFactory;
    private readonly ServerConfiguration _configuration;
    private CancellationTokenSource cancelSource;
    
    public CancellationToken CancelToken => cancelSource?.Token ?? CancellationToken.None;

    public BeaconServer(LoggerFactory loggerFactory, ServerConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger("Server");
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        cancelSource = new();
    }

    public Task StartupAsync(CancellationToken cancelToken)
    {
        // Propagate the external cancellation to the server's cancellation token source.
        cancelToken.Register(cancelSource.Cancel);
        return Task.CompletedTask;
    }

    public Task WaitForCompletionAsync()
    {
        return Task.CompletedTask;
    }

    public ValueTask HandleConsoleCommand(string command)
    {
        return ValueTask.CompletedTask;
    }

    public void SignalShutdown()
    {
        cancelSource.Cancel();
    }
}