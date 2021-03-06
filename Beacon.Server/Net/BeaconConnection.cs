using Beacon.API;
using Beacon.Server.States;
using Beacon.Server.Utils;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Beacon.Server.Net;

internal class BeaconConnection : IBeaconConnection
{
    public TcpClient Tcp { get; }
    public IServer Server { get; }
    public string RemoteAddress { get; }
    public bool IsListening { get; private set; }

    private readonly ILogger _logger;
    private IConnectionState _state;

    public BeaconConnection(IServer server, TcpClient tcp)
    {
        Tcp = tcp;
        Server = server;
        IsListening = false;
        RemoteAddress = Tcp.Client.RemoteEndPoint?.ToString() ?? "";
        _logger = server.Logger;
        _state = new HandshakeState(this);
    }

    public async Task AcceptPacketsAsync(CancellationToken cancelToken)
    {
        if (!Tcp.Connected)
            throw new IOException("The TCP client is not connected.");

        if (!Tcp.GetStream().CanRead || !Tcp.GetStream().CanWrite)
            throw new IOException("The TCP stream is not readable/writeable.");

        IsListening = true;

        while (Tcp.Connected && !cancelToken.IsCancellationRequested)
        {
            int packetId;
            MemoryStream? dataStream;
            try
            {
                dataStream = await ReadPacketAsync(cancelToken);
                if (dataStream == null)
                {
                    return;
                }
                (packetId, _) = await dataStream.ReadVarIntAsync();
            }
            catch (Exception)
            {
                return;
            }
            // Packet is received, now handle it.
            try
            {
                if (_state is null) throw new InvalidOperationException("The connection has no state!");
                await this._state.HandlePacketAsync(packetId, dataStream, cancelToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Lost connection to a client due to unhandled exception while handling packet!");
                Dispose();
                throw;
            }
        }

        IsListening = false;
    }

    public Stream Stream => Tcp.GetStream();

    public void Dispose()
    {
        this.Tcp.Dispose();
        this.IsListening = false;
    }

    public void ChangeState(IConnectionState state) => _state = state;
    private async ValueTask<MemoryStream?> ReadPacketAsync(CancellationToken cancelToken)
    {
        if (Tcp is null) return null;
        var stream = Tcp.GetStream();
        var (packetLength, _) = await stream.ReadVarIntAsync();
        if (packetLength == 0)
        {
            // Client disconencted?
            return null;
        }
        var bytes = new byte[packetLength];

        await stream.ReadAsync(bytes.AsMemory(0, packetLength), cancelToken);

        var memory = MemoryStreaming.Manager.GetStream("Connection");
        await memory.WriteAsync(bytes.AsMemory(0, packetLength), cancelToken);
        memory.Position = 0;

        return memory;
    }

}
