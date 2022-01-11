using Beacon.API;
using Beacon.Server.States;
using System.Net.Sockets;

namespace Beacon.Server.Net
{
    internal class BeaconConnection : IDisposable
    {
        public TcpClient Tcp { get; }
        public IServer Server { get; }
        public IClientState State { get; protected set; }
        public bool IsListening { get; private set; }

        public BeaconConnection(TcpClient tcp, IServer server, HandshakeState startState)
        {
            Tcp = tcp;
            Server = server;
            State = startState;
        }

        public NetworkStream GetStream() => Tcp.GetStream();
        public async Task AcceptPacketsAsync(CancellationToken cancelToken)
        {
            if (!Tcp.Connected)
                throw new IOException("The TCP client is not connected.");

            if (!Tcp.GetStream().CanRead || !Tcp.GetStream().CanWrite)
                throw new IOException("The TCP stream is not readable/writeable.");

            IsListening = true;

            while (Tcp.Connected && !cancelToken.IsCancellationRequested)
            {
                MemoryStream? dataStream = default;
                int packetId = default;
                try
                {
                    dataStream = await ReadPacketAsync(cancelToken);
                    if (dataStream == null)
                    {
                        Dispose();
                        return;
                    }
                    (packetId, _) = await dataStream.ReadVarIntAsync();
                }
                catch (Exception)
                {
                    Dispose();
                    return;
                }
                // Packet is received, now handle it.
                try
                {
                    await this.State.HandlePacketAsync(this, packetId, dataStream, cancelToken);
                }
                catch (Exception)
                {
                    throw;
                }

            }

            IsListening = false;

        }
        public void ChangeState(IClientState newState)
        {
            this.State = newState;
        }
        public void Close() => this.Dispose();
        public void Dispose()
        {
            this.Tcp.Close();
            this.Tcp.Dispose();
        }

        protected async ValueTask<MemoryStream?> ReadPacketAsync(CancellationToken cancelToken)
        {
            var stream = Tcp.GetStream();
            var (packetLength, _) = await stream.ReadVarIntAsync();
            if (packetLength == 0)
            {
                // Client disconencted?
                return null;
            }
            var bytes = new byte[packetLength];

            await stream.ReadAsync(bytes.AsMemory(0, packetLength), cancelToken);

            var memory = BeaconServer.MemoryStreamManager.GetStream("Connection");
            await memory.WriteAsync(bytes.AsMemory(0, packetLength), cancelToken);
            memory.Position = 0;

            return memory;
        }


    }
}
