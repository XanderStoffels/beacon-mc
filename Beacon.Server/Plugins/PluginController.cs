using Beacon.PluginEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Plugins
{
    internal class PluginController : IPluginController
    {
        private readonly ILogger<PluginController> _logger;
        private readonly List<IPluginLoader> _pluginLoaders;
        private readonly List<IPluginContainer> _containers;
        private IServiceProvider? _pluginServiceProvider;
        public PluginController(ILogger<PluginController> logger, IServiceProvider provider)
        {
            _logger = logger;
            _pluginLoaders = provider.GetServices<IPluginLoader>().ToList();
            _containers = new();
        }

        public async ValueTask LoadAsync()
        {
            if (_containers.Any())
                return;

            var configuredContainers = new List<IPluginContainer>();
            var services = new ServiceCollection();
            foreach (var loader in _pluginLoaders)
            {
                _logger.LogInformation("Using {name} to load plugins", loader.GetType().Name);
                var containers = await loader.LoadAsync();
                if (containers == null) continue;
                if (!containers.Any()) continue;

                // Try to add services from each loaded plugin.
                foreach (var container in containers)
                {
                    if (container.Plugin == null)
                    {
                        _logger.LogWarning("PluginContainer {name} does not contain a plugin while configuring! Is it already unloaded?", container.Name);
                        continue;
                    }

                    try
                    {
                        container.Plugin.ConfigureServices(services);
                        configuredContainers.Add(container);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Could not configure services for plugin {name}", container.Plugin.Name);
                    }
                }
            }

            _pluginServiceProvider = services.BuildServiceProvider();

            // Only enable the plugins when all plugins have registered their services.
            foreach (var container in configuredContainers)
            {
                if (container.Plugin == null)
                {
                    _logger.LogWarning("PluginContainer {name} does not contain a plugin while enabling! Is it already unloaded?", container.Name);
                    continue;
                }
                try
                {
                    await container.Plugin.EnableAsync();
                    _containers.Add(container);
                    _logger.LogInformation("Plugin {name} loaded", container.Plugin.Name);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Could not enable plugin {name}", container.Plugin.Name);
                }
            }

        }

        public async ValueTask UnloadAsync()
        {
            _pluginServiceProvider = null;

            foreach (var container in _containers)
                await (container.Plugin?.DisableAsync() ?? ValueTask.CompletedTask);

            foreach (var container in _containers)
                await container.UnloadAsync();

            _containers.Clear();

        }

        public ValueTask PublishEvent<TEvent>(TEvent e, CancellationToken cToken = default)
        {
            throw new NotImplementedException();
        }


    }
}
