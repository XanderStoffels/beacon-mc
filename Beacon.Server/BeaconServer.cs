using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Beacon.API.Events;
using Beacon.Server.Hosting;
using Beacon.Server.Net;
using Beacon.Server.Plugins;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public sealed partial class BeaconServer
{
    private readonly CancellationTokenSource _cancelSource;

    private readonly ServerEnvironment _env;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<BeaconServer> _logger;
    private readonly PluginManager _pluginManager;

    internal CancellationToken CancelToken => _cancelSource.Token;
    internal bool IsShuttingDown => CancelToken.IsCancellationRequested;

    public static Version McVersion => new(Assembly
        .GetExecutingAssembly()
        .GetCustomAttribute<MinecraftVersionAttribute>()?
        .MinecraftVersion.ToString(3) ?? string.Empty);

    public static Version? Version => Assembly
        .GetExecutingAssembly()
        .GetName()
        .Version;

    public BeaconServer(
        ILogger<BeaconServer> logger,
        IHostApplicationLifetime lifetime,
        PluginManager pluginManager,
        ServerEnvironment env)
    {
        _logger = logger;
        _lifetime = lifetime;
        _pluginManager = pluginManager;
        _env = env;
        _cancelSource = CancellationTokenSource.CreateLinkedTokenSource(lifetime.ApplicationStopping);
    }

    public IMinecraftEventBus Events => _pluginManager;
    internal async Task RunAsync()
    {
        var timer = Stopwatch.StartNew();
        await StartupAsync();
        timer.Stop();
        _logger.LogInformation("Server started in {Time}ms", timer.Elapsed.Microseconds / 1000.0);
        
        await DoServerTasksAsync();
        await ShutdownAsync();
    }
    private async Task StartupAsync()
    {
        _logger.LogInformation("Starting the server");

        // Plugins
        _logger.LogInformation("Loading plugins");
        await _pluginManager.LoadPlugins();
    }
    private async Task DoServerTasksAsync()
    {
        var commandInput = Task.Run(() =>
        {
            var _ = ReadCommandsFromInput();
        }, CancelToken);
        var acceptClients = AcceptConnectionsAsync();

        try
        {
            await Task.WhenAll(acceptClients);
        }
        catch (OperationCanceledException)
        {
            // Ignored
        }
    }
    private Task ShutdownAsync()
    {
        _logger.LogInformation("Shutting down the server");
        if (!IsShuttingDown)
            _cancelSource.Cancel();
        return Task.CompletedTask;
    }
    private async Task ReadCommandsFromInput()
    {
        var inputStream = _env.InputStream;
        try
        {
            while (!IsShuttingDown)
            {
                var cmd = await inputStream.ReadLineAsync(CancelToken) ?? string.Empty;
                await HandleDirectCommand(cmd, _env.OutputStream);
            }
        }
        catch (TaskCanceledException)
        {
            /* Ignored */
        }
        finally
        {
            _logger.LogInformation("Stopped reading direct commands");
        }
    }
    private Task HandleDirectCommand(string command, TextWriter outStream)
    {
        if (command != "stop") return Task.CompletedTask;
        _lifetime.StopApplication();
        return Task.CompletedTask;
    }
    private async Task AcceptConnectionsAsync()
    {
        _logger.LogInformation("Start listening for clients");

        var listener = new TcpListener(IPAddress.Any, _env.ServerConfiguration.Port);
        listener.Start();

        while (!IsShuttingDown)
        {
            var client = await listener.AcceptTcpClientAsync(CancelToken);
            _ = HandleTcpClient(client, CancelToken);
        }
    }
    private async Task HandleTcpClient(TcpClient tcp, CancellationToken cToken)
    {
        var connection = new BeaconConnection(this, tcp);

        // Fire event to notify plugins a TCP connection has been made.
        var e = new TcpConnectedEvent(this, connection);
        await Events.FireEventAsync(e, cToken);

        if (e.IsCancelled)
        {
            connection.Dispose();
            return;
        }

        var faulted = false;

        try
        {
            await connection.AcceptPacketsAsync(cToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "An exception occured while a connection was accepting packets");
            faulted = true;
        }
        finally
        {
            connection.Dispose();
        }

        // Fire event to notify plugins a TCP connection has been lost.
        await Events.FireEventAsync(new TcpDisconnectedEvent(this, faulted, connection.RemoteAddress), cToken);
    }
}