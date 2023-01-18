using System.Threading.Channels;
using Beacon.API;
using Beacon.Server.Net;
using Beacon.Server.Net.Packets;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;

namespace Beacon.Server;

public class BeaconServer : IServer
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ServerConfiguration _configuration;
    private readonly ClientReceiver _clientReceiver;
    private readonly Channel<QueuedServerboundPacket> _incomingPacketChannel;
    private readonly CancellationTokenSource _cancelSource;
    
    public ILogger Logger { get; }
    public ServerStatus Status { get; }
    
    public CancellationToken CancelToken => _cancelSource.Token;
    public ChannelWriter<QueuedServerboundPacket> IncomingPacketsChannel => _incomingPacketChannel.Writer;

    public BeaconServer(ILoggerFactory loggerFactory, ServerConfiguration configuration)
    {
        Logger = loggerFactory.CreateLogger("Server");
        _cancelSource = new();
        _loggerFactory = loggerFactory;
        _configuration = configuration;
        _clientReceiver = new(configuration.Port, 30, Logger);
        _incomingPacketChannel = Channel.CreateUnbounded<QueuedServerboundPacket>(new()
        {
            SingleReader = true
        });

        Status = new ServerStatus
        {
            Version = new ServerVersionModel
            {
                Name = "1.19.3",
                Protocol = 761
            },
            Players = new OnlinePlayersModel
            {
                Max = 10,
                Online = 1,
                Sample = new[]
                {
                    new OnlinePlayerModel
                    {
                        Name = "Notch",
                        Id = new Guid().ToString()
                    }
                }
            },
            Description = new DescriptionModel
            {
                Text = "A Beacon Server!"
            },
            Favicon = Resources.ServerIcon
        };
    }

    public Task StartupAsync(CancellationToken cancelToken)
    {
        Logger.LogInformation("Starting Beacon");
        
        // Propagate the external cancellation to the server's cancellation token source.
        cancelToken.Register(_cancelSource.Cancel);
        
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
            if (timeLeft < 0) Logger.LogWarning("Can not keep up! ({Time}ms behind)", -timeLeft);
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
        while (_incomingPacketChannel
               .Reader
               .TryRead(out var message))
        {
            await message.Packet.HandleAsync(this, message.Connection);
            Logger.LogDebug("[{IP}] [{State}] Handled packet with ID {PacketId}", 
                message.Connection.Ip, 
                message.Connection.State, 
                message.Packet.Id);
        }
    }

    private void HandleNewConnection(CancellationToken cancelToken)
    {
        if (!_clientReceiver.ClientQueue.TryRead(out var client)) return;
        if (!client.Connected) return; // The client might have disconnected while in queue.
        var connection = new ClientConnection(client, this, Logger);
        Logger.LogDebug("[{IP}] Accepted connection", connection.RemoteEndPoint?.ToString());

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