using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Net;

public sealed class ConnectionManager
{
    private readonly int _port;
    private readonly ILogger _logger;
    private readonly Channel<TcpClient> _channel;
    
    public ChannelReader<TcpClient> ClientQueue => _channel.Reader;
    private ChannelWriter<TcpClient> Writer => _channel.Writer;

    public ConnectionManager(int port, ILogger logger)
    {
        _port = port;
        _logger = logger;
        _channel = Channel.CreateUnbounded<TcpClient>();
    }
    
    public ConnectionManager(int port, int maxQueuedConnections, ILogger logger)
    {
        _port = port;
        // Create a queue for connections. If the queue is full, new connections will be dropped.
        _channel = Channel.CreateBounded<TcpClient>(new BoundedChannelOptions(maxQueuedConnections)
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
        
        while (!cancelToken.IsCancellationRequested)
        {
            try
            {
                var client = await listener.AcceptTcpClientAsync(cancelToken);
                if (Writer.TryWrite(client)) continue;
                
                // If the queue is full, close the connection.
                client.Close();
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}