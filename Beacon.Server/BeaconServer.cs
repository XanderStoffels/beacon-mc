using Beacon.API;
using Beacon.API.Models;
using Beacon.Server.Net;
using Beacon.Server.Plugins;
using Beacon.Server.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Beacon.Server
{
    internal class BeaconServer : BackgroundService, IServer
    {
        public static readonly RecyclableMemoryStreamManager MemoryStreamManager = new();
        public static readonly Version Version = new(0, 1);

        private readonly ILogger<BeaconServer> _logger;
        private readonly IServiceProvider _provider;
        private readonly IPluginController _plugins;

        public BeaconServer(ILogger<BeaconServer> logger, IServiceProvider provider, IPluginController plugins)
        {
            _logger = logger;
            _provider = provider;
            _plugins = plugins;
        }

        public ValueTask<ServerStatus> GetStatusAsync()
        {
            return ValueTask.FromResult(new ServerStatus()
            {
                Version = new ServerVersionModel
                {
                    Name = "1.18.1",
                    Protocol = 757
                },
                Players = new OnlinePlayersModel
                {
                    Max = 9,
                    Online = 6,
                    Sample = new OnlinePlayerModel[]
                    {
                        new OnlinePlayerModel
                        {
                            Name = "Gottem",
                            Id = new Guid().ToString(),
                        }
                    }
                },
                Description = new DescriptionModel
                {
                    Text = "A server running on Beacon!"
                },
                Favicon = "data:image/png;base64,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"
            });
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
#if DEBUG
            _logger.LogWarning("Running in debug mode! Performance will be impaired");
#else
            _logger.LogInformation("Running in production mode");
#endif
            _logger.LogDebug("Debug logs are enabled");
            // Plugins
            var watch = Stopwatch.StartNew();
            await _plugins.InitializePlugins();

            watch.Stop();
            _logger.LogInformation("Server booted in {seconds} seconds", watch.ElapsedMilliseconds / 1000.0);
            _logger.LogInformation("Start listening for clients");
            var listener = new TcpListener(IPAddress.Any, 25565);
            listener.Start();

            while (!cancelToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync();
                _logger.LogDebug("{ip} requested the server status", client.Client.RemoteEndPoint?.ToString() ?? "A client");
                var con = new BeaconConnection(client, this, _provider.GetRequiredService<HandshakeState>());
                _ = con.AcceptPacketsAsync(cancelToken);
            }
        }
    }
}
