namespace Beacon.Server.Exceptions;

public class PluginLoadException : Exception
{
    public PluginLoadException(string? message) : base(message)
    {
    }

    public PluginLoadException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}