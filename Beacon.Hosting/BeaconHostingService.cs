using Beacon.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beacon.Hosting;

public class BeaconHostingService : BackgroundService
{
    private readonly IBeaconStartup _startup;
    private readonly LoggerFactory _loggerFactory;
    private BeaconServer? _server;
    
    
    public BeaconHostingService(IBeaconStartup startup, LoggerFactory loggerFactory)
    {
        _startup = startup;
        _loggerFactory = loggerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = await _startup.LoadConfigurationAsync();

        _server = new(_loggerFactory, config);
        await _server.StartupAsync(stoppingToken);
    }
}