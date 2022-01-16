
using Beacon.API.Commands;

namespace Beacon.Server.Plugins;

public interface IPluginController
{
    public ValueTask LoadAsync();
    public ValueTask UnloadAsync();
    public List<ICommand> GetRegisteredCommands();
}
