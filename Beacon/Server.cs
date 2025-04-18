using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Beacon.Config;
using Beacon.Net;
using Beacon.Net.Packets.Status;

namespace Beacon;

public class  Server : BackgroundService
{
    private readonly ILogger<Server> _logger;
    private readonly ServerConfiguration _config;
    
    private readonly Channel<QueuedServerBoundPacket> _serverBoundPackets;
    private readonly ILoggerFactory _loggerFactory;

    public Server(ILogger<Server> logger, ServerConfiguration config, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _config = config;
        _serverBoundPackets = Channel.CreateUnbounded<QueuedServerBoundPacket>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Server started on port {Port}", _config.Port);

        var acceptClients = AcceptClients(stoppingToken);
        DoGameLoop(stoppingToken);
        
        await Task.WhenAll(acceptClients);
        _logger.LogInformation("Server has shut down");
    }


    private void DoGameLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var counter = 0;
            
            // Read all the incoming packets that need to be handled.
            while (_serverBoundPackets.Reader.TryRead(out QueuedServerBoundPacket queuedPacket))
            {
                try
                {
                    counter++;
                    queuedPacket.Packet.Handle(this, queuedPacket.Origin);
                    if (queuedPacket.Packet is IDisposable disposablePacket)
                        disposablePacket.Dispose();
                }
                catch (Exception e)
                {
                    var packetName = queuedPacket.Packet.GetType().Name;
                    _logger.LogError(e, "Unhandled exception handling packet {PacketName}", packetName);
                }
            }
            Thread.Sleep(50);
        //    Console.WriteLine(counter);
        }
    }
    
    private async Task HandleClient(TcpClient client, CancellationToken cancelToken)
    {
        var endpoint = client.Client.RemoteEndPoint?.ToString();
        try
        {
            var logger = _loggerFactory.CreateLogger<Connection>();
            using var connection = new Connection(this, client, logger);
            await connection.SendAndReceivePackets(_serverBoundPackets.Writer, cancelToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client connection");
        }
        
        _logger.LogInformation("Client disconnected from {RemoteEndPoint}", endpoint);
    }
    
  
    private async Task AcceptClients(CancellationToken stoppingToken)
    {
        using var clientListener = new TcpListener(IPAddress.Any, _config.Port);
        clientListener.Start();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = await clientListener.AcceptTcpClientAsync(stoppingToken);
                _logger.LogInformation("Client connected from {RemoteEndPoint}", client.Client.RemoteEndPoint);
                _ = HandleClient(client, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error accepting client connection");
            }
        }

        clientListener.Stop();
    }

}