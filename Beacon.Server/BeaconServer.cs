using System.Threading.Channels;
using Beacon.API;
using Beacon.Server.Net;
using Beacon.Server.Net.Packets;
using Beacon.Server.Net.Packets.Status.ServerBound;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public sealed partial class BeaconServer : IServer
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ServerConfiguration _configuration;
    private readonly ClientReceiver _clientReceiver;
    private readonly Channel<QueuedServerBoundPacket> _incomingPacketChannel;
    private readonly CancellationTokenSource _cancelSource;
    
    public ServerStatus Status { get; }
    public CancellationToken CancelToken => _cancelSource.Token;
    public ChannelWriter<QueuedServerBoundPacket> IncomingPacketsChannel => _incomingPacketChannel.Writer;

    public BeaconServer(ILoggerFactory loggerFactory, ServerConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger("Server");
        _cancelSource = new();
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        _clientReceiver = new(configuration.Port, 30, _logger);
        _incomingPacketChannel = Channel.CreateUnbounded<QueuedServerBoundPacket>(new()
        {
            SingleReader = true
        });

        Status = ServerStatus.Default;
    }

    public Task StartupAsync(CancellationToken cancelToken)
    {
        _logger.LogInformation("Starting Beacon");
        
        // Propagate the external cancellation to the server's cancellation token source.
        cancelToken.Register(_cancelSource.Cancel);
        
        // Load plugins.
        
        
        // Start server tasks.
        var tcpTask = _clientReceiver.AcceptClientsAsync(cancelToken);
        var loopTask = DoGameLoopAsync(cancelToken);
        
        var _ = Task.WhenAll(tcpTask, loopTask);
        return Task.CompletedTask;
    }
    public void WaitForCompletion()
    {
         _cancelSource.Token.WaitHandle.WaitOne();
    }
    public ValueTask HandleConsoleCommand(string command)
    {
        Console.WriteLine(command);
        return ValueTask.CompletedTask;
    }
    public void SignalShutdown()
    {
        _cancelSource.Cancel();
    }

    private async Task DoGameLoopAsync(CancellationToken cancelToken)
    {
        var timer = new GameTimer(1000 / 20);

        while (!cancelToken.IsCancellationRequested)
        {
            var timeLeft = await timer.WaitForNextTickAsync();
            if (timeLeft < 0) _logger.LogWarning("Can not keep up! ({Time}ms behind)", -timeLeft);
            await Update();
        }

        async Task Update()
        {
           HandleNewConnection(cancelToken);
           await HandlePackets();
        }
    }
    private async Task HandlePackets()
    {
        var reader = _incomingPacketChannel.Reader;
        while (reader.TryRead(out var message))
        {
            await message.Packet.HandleAsync(this, message.Connection);
            message.Dispose();

            var time = DateTime.Now - message.QueuedAt;
            LogPacketHandled(message.Connection.Ip, message.Connection.State, message.Packet.Id, time.Milliseconds);
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