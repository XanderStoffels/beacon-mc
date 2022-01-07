using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Beacon.API.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Plugins
{
    internal class PluginController : IPluginController
    {
        private readonly IPluginDiscoverer _loader;
        private readonly ILogger<PluginController> _logger;
        private readonly List<IBeaconPlugin> _loadedPlugins;

        public IServiceProvider? _pluginServices;
        public bool IsInitialized => _pluginServices != null;

        public PluginController(IPluginDiscoverer loader, ILogger<PluginController> logger)
        {
            _loader = loader;
            _logger = logger;
            _loadedPlugins = new();
        }

        public async ValueTask InitializePlugins()
        {
            if (IsInitialized) return;

            _loadedPlugins.Clear();
            _logger.LogInformation("Loading plugins");

            var faultedPlugins = new List<IBeaconPlugin>();
            var plugins = _loader.DiscoverPlugins();
            _logger.LogInformation("{amount} plugins discovered", plugins.Count);

            // These services will contain the event handlers.
            // Any custom services to provide to plugins can be registered here.
            var services = new ServiceCollection();
            foreach (var plugin in plugins)
            {
                _logger.LogDebug("Configuring services for plugin {pluginname}", plugin.Name);
                try
                {
                    plugin.RegisterServices(services);
                }
                catch (Exception e)
                {
                    faultedPlugins.Add(plugin);
                    _logger.LogWarning(e, "{pluginname} threw an exception while configuring services", plugin.Name);
                }
            }

            plugins = plugins.Except(faultedPlugins).ToList();
            faultedPlugins.Clear();

            _pluginServices = services.BuildServiceProvider();

            foreach (var plugin in plugins)
            {
                _logger.LogDebug("Enabling {pluginname}", plugin.Name);
                try
                {
                    await plugin.Enable();
                }
                catch (Exception e)
                {
                    faultedPlugins.Add(plugin);
                    _logger.LogWarning(e, "{pluginname} threw an exception while enabling", plugin.Name);
                }
            }

            plugins = plugins.Except(faultedPlugins).ToList();
            _loadedPlugins.AddRange(plugins);
            _logger.LogInformation("{amount} plugins loaded", _loadedPlugins.Count);
        }

        public List<IMinecraftEventHandler<TEvent>> GetEventHandlers<TEvent>() where TEvent : MinecraftEvent
            => _pluginServices == null 
                ? new() 
                : _pluginServices.GetServices<IMinecraftEventHandler<TEvent>>().ToList();

        public async ValueTask<TEvent> FireEventAsync<TEvent>(TEvent e, CancellationToken cToken = default) where TEvent : MinecraftEvent
        {
            _logger.LogDebug("Firing event {eventname}", e.GetType().Name);
            foreach (var handler in GetEventHandlers<TEvent>())
            {
                if (cToken.IsCancellationRequested)
                    e.IsCancelled = true;

                if (e.IsCancelled)
                    return e;

                await handler.HandleAsync(e, cToken);
            }
            return e;
        }

 
    }
}
