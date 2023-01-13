using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Net;

public sealed class ClientReceiver
{
    private readonly int _port;
    private readonly ILogger _logger;
    private readonly Channel<ClientConnection> _channel;
    
    public ChannelReader<ClientConnection> ClientQueue => _channel.Reader;
    private ChannelWriter<ClientConnection> Writer => _channel.Writer;

    public ClientReceiver(int port, ILogger logger)
    {
        _port = port;
        _logger = logger;
        _channel = Channel.CreateUnbounded<ClientConnection>();
    }
    
    public ClientReceiver(int port, int maxQueuedConnections, ILogger logger)
    {
        _port = port;
        // Create a queue for connections. If the queue is full, new connections will be dropped.
        _channel = Channel.CreateBounded<ClientConnection>(new BoundedChannelOptions(maxQueuedConnections)
        {
            SingleWriter = true,
            Capacity = maxQueuedConnections,
            FullMode = BoundedChannelFullMode.DropWrite
        });
        _logger = logger;

    }

    public async Task AcceptClientsAsync(CancellationToken cancelToken)
    {
        var listener = new TcpListener(IPAddress.Any, _port);
        listener.Start();
        _logger.LogInformation("Listening for connections on port {Port}", _port);
        while (!cancelToken.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(cancelToken);
                var connection = new ClientConnection(client);
                if (Writer.TryWrite(connection)) continue;

                // If the queue is full, close the connection.
                _logger.LogWarning("ClientConnection queue is full. Dropping connection from {@Client}",
                    client.Client.RemoteEndPoint);
                client.Close();
            }
            catch (OperationCanceledException)
            {
                // Ignored. This is expected when the server is shutting down.
                break;
            }
        }
        
        listener.Stop();
        _logger.LogInformation("Stopped listening for clients");
    }
}