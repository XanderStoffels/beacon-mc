using Beacon.API.Plugins;

namespace Beacon.Plugins;

public interface IPluginContainer : IAsyncDisposable
{
    public string Name { get; }
    public IBeaconPlugin? Plugin { get; }
    public ValueTask UnloadAsync();
}
