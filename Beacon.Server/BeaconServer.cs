using Beacon.API;
using Beacon.Server.Net;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public class BeaconServer : IServer
{
    private readonly ILogger _logger;
    private readonly LoggerFactory _loggerFactory;
    private readonly ServerConfiguration _configuration;
    private readonly ConnectionManager _connectionManager;
    private CancellationTokenSource cancelSource;
    
    public CancellationToken CancelToken => cancelSource?.Token ?? CancellationToken.None;

    public BeaconServer(LoggerFactory loggerFactory, ServerConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger("Server");
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        _connectionManager = new(configuration.Port, _logger);
        cancelSource = new();
    }

    public Task StartupAsync(CancellationToken cancelToken)
    {
        _logger.LogInformation("Starting Beacon");
        
        // Propagate the external cancellation to the server's cancellation token source.
        cancelToken.Register(cancelSource.Cancel);
        return Task.CompletedTask;
        
        
    }

    public void WaitForCompletion()
    {
         cancelSource.Token.WaitHandle.WaitOne();
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