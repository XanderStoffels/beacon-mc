using Beacon.API.Events;
using Beacon.API.Models;

namespace Beacon.API;

public interface IServer
{
    public IMinecraftEventBus Events { get; }
    public ServerStatus GetStatus();
}