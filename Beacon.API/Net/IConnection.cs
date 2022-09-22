
namespace Beacon.API.Net;

public interface IConnection : IDisposable
{
    public IServer Server { get; }
    public Stream Stream { get; }
    public string RemoteAddress { get; }
}
