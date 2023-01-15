namespace Beacon.API;

public interface IServer
{
    ServerStatus Status { get; }
    void SignalShutdown();
}