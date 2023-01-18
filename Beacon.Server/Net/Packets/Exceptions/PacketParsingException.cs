namespace Beacon.Server.Net.Packets.Exceptions;

public class PacketParsingException : Exception
{
    public PacketParsingException(string? message) : base(message)
    {
    }

    public PacketParsingException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public static void Throw(string propertyName, string packetName)
    {
        throw new PacketParsingException($"Could not parse property {propertyName} for packet {propertyName}");
    }
}