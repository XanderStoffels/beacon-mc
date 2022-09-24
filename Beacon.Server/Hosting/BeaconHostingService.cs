using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Hosting;

internal sealed class BeaconHostingService : BackgroundService
{
    private readonly HostingConfiguration _configuration;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<BeaconHostingService> _logger;
    private readonly BeaconServer _server;

    public BeaconHostingService(ILogger<BeaconHostingService> logger,
        HostingConfiguration configuration,
        BeaconServer server,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _configuration = configuration;
        _server = server;
        _lifetime = lifetime;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
#if DEBUG
        _logger.LogWarning("This binary is compiled using a Debug configuration! Performance will be impaired");
#endif
        try
        {
            await _server.RunAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "It looks like the server has crashed...");
        }

        if (_configuration.ExitOnServerShutdown)
        {
            _logger.LogInformation("Shutting down the host");
            _lifetime.StopApplication();
        }
    }
}