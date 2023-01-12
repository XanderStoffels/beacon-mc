using Beacon.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beacon.Hosting;

public class BeaconHostingService : BackgroundService
{
    private readonly IBeaconStartup _startup;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private BeaconServer? _server;
    
    
    public BeaconHostingService(IBeaconStartup startup, ILoggerFactory loggerFactory, ILogger<BeaconHostingService> logger)
    {
        _startup = startup;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.Register(() => _logger.LogInformation("Beacon Host received the stopping signal"));
        
        var config = await _startup.LoadConfigurationAsync();
        _server = new(_loggerFactory, config);
        
        await _server.StartupAsync(stoppingToken);
        _ = Task.Run(async () => await ProvideConsoleCommands(stoppingToken), stoppingToken);
    }

    private async Task ProvideConsoleCommands(CancellationToken stoppingToken)
    {
        if (_server == null) return;
        await foreach (var command in _startup.ProvideConsoleCommands(stoppingToken))
        {
            try
            {
                await _server.HandleConsoleCommand(command);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Stopped reading console commands because of cancellation");
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception handling console command '{Command}'", command);
            }
        }
    }
}