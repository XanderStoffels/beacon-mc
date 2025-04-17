using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Beacon.Config;
using Beacon.Net;
using Beacon.Net.Packets;
using Beacon.Net.Packets.Status;

namespace Beacon;

public class Server : BackgroundService
{
    private readonly ILogger<Server> _logger;
    private readonly ServerConfiguration _config;
    
    private readonly Channel<QueuedServerBoundPacket> _serverBoundPackets;
    private readonly Channel<QueuedClientBoundPacket> _clientBoundPackets;

    public Server(ILogger<Server> logger, ServerConfiguration config)
    {
        _logger = logger;
        _config = config;
        _serverBoundPackets = Channel.CreateUnbounded<QueuedServerBoundPacket>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _clientBoundPackets = Channel.CreateUnbounded<QueuedClientBoundPacket>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public bool EnqueuePacket(IClientBoundPacket packet, Connection target)
    {
        if (_clientBoundPackets.Writer.TryWrite(new QueuedClientBoundPacket(packet, target)))
        {
            _logger.LogDebug("Enqueued packet {PacketId}", nameof(packet));
            return true;
        }
        
        _logger.LogWarning("Failed to enqueue packet {PacketId}", nameof(packet));
        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _logger.LogInformation("Server started on port {Port}", _config.Port);

        var acceptClients = AcceptClients(stoppingToken);
        var processClientPackets = ProcessClientBoundPackets(stoppingToken);
        DoGameLoop(stoppingToken);
        
        await Task.WhenAll(acceptClients, processClientPackets);
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
                }
                catch (Exception e)
                {
                    var packetName = queuedPacket.Packet.GetType().Name;
                    _logger.LogError(e, "Unhandled exception handling packet {PacketName}", packetName);
                }
            }
            Thread.Sleep(50);
            Console.WriteLine(counter);
        }
    }
    
    private async Task HandleClient(TcpClient client, CancellationToken cancelToken)
    {
        var endpoint = client.Client.RemoteEndPoint?.ToString();
        try
        {
            using var connection = new Connection(this, client);
            await connection.AcceptPacketsAsync(_serverBoundPackets.Writer, cancelToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling client connection");
        }
        
        _logger.LogInformation("Client disconnected from {RemoteEndPoint}", endpoint);
    }
    
    private async Task ProcessClientBoundPackets(CancellationToken cancelToken)
    {
        const int twoMegabytes = 2 * 1024 * 1024;
        
        // The prefix buffer is used to write the VarInt length of the payload. Max length is 5 bytes.
        Memory<byte> prefixBuffer = new byte[5];
        Memory<byte> buffer = new byte[twoMegabytes];
        
        await foreach (var (packet, connection) in _clientBoundPackets.Reader.ReadAllAsync(cancelToken))
        {
            try
            { 
                // The payload is all the bytes after the initial VarInt.
                // Before writing the payload, we need to write the VarInt length of the payload.
                if (!packet.TryWritePayloadTo(buffer.Span, out var payloadLength))
                {
                    _logger.LogWarning("Failed to write payload of {PacketId} to buffer", packet.GetType().Name);
                    continue;
                }
                VarInt.TryWrite(prefixBuffer.Span, payloadLength, out var prefixLength); 
                
                // TODO: Does making this sync make it faster?
                var stream = connection.Tcp.GetStream();
                await stream.WriteAsync(prefixBuffer[..prefixLength], CancellationToken.None);
                await stream.WriteAsync(buffer[..payloadLength], CancellationToken.None);
                await stream.FlushAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing server bound packet");
            }
        }
        
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