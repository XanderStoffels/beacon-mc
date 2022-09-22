using Beacon.API.Events;
using Beacon.API.Models;

namespace Beacon.API
{
    public interface IServer
    {
        public static Version Version { get; } = new(0,0,0);
        public ValueTask<ServerStatus> GetStatusAsync();
        public IMinecraftEventBus EventBus { get; }
    }
}
