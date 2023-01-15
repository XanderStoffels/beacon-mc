﻿using System.Diagnostics;
using Beacon.API;
using Beacon.Server.Net;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public class BeaconServer : IServer
{
    private readonly ILogger _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ServerConfiguration _configuration;
    private readonly ClientReceiver _clientReceiver;
    private CancellationTokenSource cancelSource;
    
    public ServerStatus Status { get; }
    public CancellationToken CancelToken => cancelSource?.Token ?? CancellationToken.None;

    public BeaconServer(ILoggerFactory loggerFactory, ServerConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger("Server");
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        _clientReceiver = new(configuration.Port, 30, _logger);
        cancelSource = new();

        Status = new()
        {
            Favicon = Resources.ServerIcon,
            Description = new()
            {
                Text = "A Beacon Server"
            },
            Players = new()
            {
                Max = 100,
                Online = 0,
                Sample = new()
            },
            EnforcesSecureChat = false,
            PreviewsChat = false,
            Version = new()
            {
                Name = "1.19.3",
                Protocol = 761
            }
        };
    }

    public Task StartupAsync(CancellationToken cancelToken)
    {
        _logger.LogInformation("Starting Beacon");
        
        // Propagate the external cancellation to the server's cancellation token source.
        cancelToken.Register(cancelSource.Cancel);
        
        // Start server tasks.
        var tcpTask = _clientReceiver.AcceptClientsAsync(cancelToken);
        var loopTask = DoGameLoopAsync(cancelToken);
        
        var _ = Task.WhenAll(tcpTask, loopTask);
        return Task.CompletedTask;
    }

    public void WaitForCompletion()
    {
         cancelSource.Token.WaitHandle.WaitOne();
    }
    public ValueTask HandleConsoleCommand(string command)
    {
        Console.WriteLine(command);
        return ValueTask.CompletedTask;
    }
    
    public void SignalShutdown()
    {
        cancelSource.Cancel();
    }

    private async Task DoGameLoopAsync(CancellationToken cancelToken)
    {
        var timer = new GameTimer(1000 / 20);

        while (!cancelToken.IsCancellationRequested)
        {
            var timeLeft = await timer.WaitForNextTickAsync();
            if (timeLeft < 0) _logger.LogWarning("Can not keep up! ({Time}ms behind)", -timeLeft);
            Update();
        }

        void Update()
        {
           HandleNewConnection(cancelToken);
           
           // Handle incoming packets.
           
        }
    }

    private void HandleNewConnection(CancellationToken cancelToken)
    {
        if (!_clientReceiver.ClientQueue.TryRead(out var client)) return;
        if (!client.Connected) return; // The client might have disconnected while in queue.
        var connection = new ClientConnection(client, this, _logger);
        _logger.LogDebug("[{IP}] Accepted connection", connection.RemoteEndPoint?.ToString());

        try
        {
            Task.Run(() => connection.AcceptPacketsAsync(cancelToken), cancelToken);
        }
        catch (TaskCanceledException)
        {
            // Ignored.
        }
    }
}