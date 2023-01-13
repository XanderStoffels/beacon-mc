using System.Net;
using System.Net.Sockets;

namespace Beacon.Server.Net;

public class ClientConnection
{
    private readonly TcpClient _tcp;
    
    public EndPoint? RemoteEndPoint => _tcp.Client.RemoteEndPoint;

    public ClientConnection(TcpClient tcp)
    {
        _tcp = tcp;
    }

    public Task AcceptPacketsAsync()
    {
        return Task.CompletedTask;
    }
}