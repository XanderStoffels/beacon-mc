using Beacon.API;
using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Beacon.API.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Plugins
{
    internal class PluginController : IPluginController
    {
        private readonly IPluginLoader _loader;
        private readonly ILogger<PluginController> _logger;
        private readonly List<IPluginContext> _loadedContexts;
        private IServiceProvider? _pluginServices;

        public bool IsInitialized => _pluginServices != null;

        public PluginController(IPluginLoader loader, ILogger<PluginController> logger)
        {
            _loader = loader;
            _logger = logger;
            _loadedContexts = new();
        }

        public async ValueTask LoadAsync()
        {
            if (IsInitialized) return;

            _loadedContexts.Clear();
            _logger.LogInformation("Loading plugins");

            var faultedPlugins = new List<IPluginContext>();
            var contexts = await _loader.LoadPluginContexts();
            _logger.LogInformation("{amount} plugins discovered", contexts.Count);

            // Add services to and from plugins.
            var services = ConfigurePluginServices(new ServiceCollection());
            foreach (var context in contexts)
            {
                _logger.LogDebug("Configuring services for plugin {pluginname}", context.Plugin.Name);
                try
                {
                    context.Plugin.RegisterServices(services);
                }
                catch (Exception e)
                {
                    faultedPlugins.Add(context);
                    _logger.LogWarning(e, "{pluginname} threw an exception while configuring services", context.Plugin.Name);
                }
            }

            // Clear out the plugins that crashed while adding services.
            contexts = contexts.Except(faultedPlugins).ToList();
            faultedPlugins.Clear();

            _pluginServices = services.BuildServiceProvider();

            // Enable all plugins that loaded correctly registered their services.
            foreach (var context in contexts)
            {
                _logger.LogDebug("Enabling {pluginname}", context.Plugin.Name);
                try
                {
                    await context.Plugin.Enable();
                }
                catch (Exception e)
                {
                    faultedPlugins.Add(context);
                    _logger.LogWarning(e, "{pluginname} threw an exception while enabling!", context.Plugin.Name);
                }
            }

            contexts = contexts.Except(faultedPlugins).ToList();
            _loadedContexts.AddRange(contexts);
            _logger.LogInformation("{amount} plugins loaded", _loadedContexts.Count);
        }

        public IReadOnlyList<IMinecraftEventHandler<TEvent>> GetPluginEventHandlers<TEvent>() where TEvent : MinecraftEvent
            => _pluginServices == null 
                ? new()
                : _pluginServices.GetServices<IMinecraftEventHandler<TEvent>>().ToList();

        public async ValueTask UnloadAll()
        {
            _logger.LogInformation("Unloading all plugins");
            _pluginServices = null;
            foreach (var context in _loadedContexts)
            {
                _logger.LogInformation("Unloading plugin {name}", context.Plugin.Name);
                await context.DisposeAsync();
            }
            _loadedContexts.Clear();
        }

        public async ValueTask ReloadAsync()
        {
            // Unload plugins.
            await UnloadAll();

            // Load plugins.
            //await LoadAsync();
        }

        private IServiceCollection ConfigurePluginServices(IServiceCollection services)
        {
            return services;
        }

       
    }
}
