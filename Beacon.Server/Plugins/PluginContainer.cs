using Beacon.API.Plugins;

namespace Beacon.Server.Plugins;

public class PluginContainer
{
    private readonly IPlugin _plugin;
    
    public PluginContainer(IPlugin plugin)
    {
        _plugin = plugin;
    }
    
}