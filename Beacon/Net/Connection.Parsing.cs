using System.Buffers;
using Beacon.Net.Packets;
using Beacon.Net.Packets.Configuration.ClientBound;
using Beacon.Net.Packets.Configuration.ServerBound;
using Beacon.Net.Packets.Handshaking.ServerBound;
using Beacon.Net.Packets.Login.ServerBound;
using Beacon.Net.Packets.Status.ServerBound;

namespace Beacon.Net;

public sealed partial class Connection
{
    private bool _expectLoginAck;
    private bool _expectConfigurationAck;
    public List<KnownPack> KnownPacks { get; set; } = [];

    private IServerBoundPacket? ParsePacketData(ref SequenceReader<byte> reader, int packetId)
    {
        switch (State)
        {
            case ConnectionState.Handshaking:
                ParseHandshakingPacket(ref reader, packetId);
                return null;
            case ConnectionState.Status:
                ParseStatusPacket(ref reader, packetId);
                return null;
            case ConnectionState.Login:
                ParseLoginPacket(ref reader, packetId);
                return null;
            case ConnectionState.Configuration:
                return ParseConfigurationPacket(ref reader, packetId);
            case ConnectionState.Transfer:
            case ConnectionState.Play:
            default:
                throw new NotImplementedException($"State {State} is not implemented");
        }
    }

    /// <summary>
    /// Handshaking packets do not change the state of the world, so they are handled immediately.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="packetId"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ParseHandshakingPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case Handshake.PacketId:
            {
                using var handshake = Handshake.Rent();
                handshake.DeserializePayload(ref reader);
                State = (ConnectionState)handshake.NextState;
                return;
            }
            case 0xFE:
            {
                // This is a legacy packet.
                var stream = Tcp.GetStream();
                stream.WriteByte(0xFF);
                _shouldClose = true;
                return;
            }
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Handshaking is not implemented");
    }

    /// <summary>
    /// Status packets do not change the state of the world, so they are handled immediately.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="packetId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private void ParseStatusPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case StatusRequest.PacketId:
                // Don't clutter the game loop with this packet. It does not affect the game state.
                StatusRequest.Instance.Handle(_server, this);
                return;

            case PingRequest.PacketId:
                // Instead of creating a new packet here, we can just inline the logic because it's so simple.
                if (reader.TryReadLong(out var timestamp))
                    PingRequest.WritePong(Tcp.GetStream(), timestamp);
                return;
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Status is not implemented");
    }

    /// <summary>
    /// Login packets do not change the state of the world, so they are handled immediately.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="packetId"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ParseLoginPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case Hello.PacketId:
            {
                _expectLoginAck = true;
                using var hello = Hello.Rent();
                hello.DeserializePayload(ref reader);
                hello.Handle(_server, this);
                return;
            }

            case 0x03 when _expectLoginAck:
            {
                _expectLoginAck = false;
                State = ConnectionState.Configuration;

                if (!_configurationKeepAliver.IsRunning)
                    _configurationKeepAliver.Start();

                var knownCorePack = KnownPack.MinecraftCore;
                var knownPacks = SelectKnownPacksFromServer.Rent();
                knownPacks.Packs.Clear();
                knownPacks.Packs.Add(knownCorePack);
                EnqueuePacket(knownPacks);
                return;
            }
            case 0x03:
            {
                _shouldClose = true;
                return;
            }
            
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Login is not implemented");
    }
    
    private AckFinishConfiguration? ParseConfigurationPacket(ref SequenceReader<byte> reader, int packetId)
    {
        switch (packetId)
        {
            case ClientInformation.PacketId:
            {
                using var packet = ClientInformation.Rent();
                packet.DeserializePayload(ref reader);
                packet.Handle(_server, this);
                
                // We can now complete the configuration process.
                _expectConfigurationAck = true;
                var finishPacket = FinishConfiguration.Rent();
                EnqueuePacket(finishPacket);
                return null;
            }
            case CustomPayloadFromClient.PacketId:
            {
                using var packet = CustomPayloadFromClient.Rent();
                packet.DeserializePayload(ref reader);
                packet.Handle(_server, this);
                return null;
            }
            case AckFinishConfiguration.PacketId:
            {
                if (!_expectConfigurationAck)
                {
                    _logger.LogWarning("Received configuration ack, but we didn't ask for it. Closing connection");
                    _shouldClose = true;
                    return null;
                }
                _configurationKeepAliver.Stop();
                State = ConnectionState.Play;
                return AckFinishConfiguration.Instance;
            }
            case 0x04: // Keep Alive response from the client.
            {
                if (reader.TryReadLong(out var timestamp) && _configurationKeepAliver.KeepAlive(timestamp)) 
                    return null;
                _logger.LogWarning($"Received invalid keep alive response from client. Closing connection");
                _shouldClose = true;
                return null;
            }
            case SelectKnownPacksFromClient.PacketId:
            {
                using var incoming = SelectKnownPacksFromClient.Rent();
                incoming.DeserializePayload(ref reader);
                KnownPacks.AddRange(incoming.Packs);
                return null;
            }
        }

        throw new NotImplementedException($"Parsing packet id {packetId} for state Configuration is not implemented");
    }
}