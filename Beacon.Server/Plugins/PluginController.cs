using Beacon.API.Commands;
using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Beacon.PluginEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Plugins;

internal class PluginController : IPluginController, IMinecraftEventBus
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
            List<IPluginContainer>? containers = default;
            try
            {
                containers = await loader.LoadAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Plugin loader {name} has encountered an error while loading plugins", loader.GetType().Name);
                continue;
            }

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

        _logger.LogInformation("{amount} plugins loaded", _containers.Count);
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

    public async ValueTask FireEventAsync<TEvent>(TEvent e, CancellationToken cToken = default) where TEvent : MinecraftEvent
    {
        if (_pluginServiceProvider == null) return;

        var handlers = _pluginServiceProvider.GetServices<IMinecraftEventHandler<TEvent>>();
        var ce = e as ICancelable; // TODO: There has to be a better way. Will do for now I guess.

        foreach (var handler in handlers)
        {
            if (cToken.IsCancellationRequested)
            {
                if (ce is not null)
                    ce.IsCancelled = true;
                return;
            }

            try
            {
                await handler.HandleAsync(e, cToken);
            }
            catch (TaskCanceledException)
            {
                if (ce is not null)
                    ce.IsCancelled = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Handler {name} threw an exception while handling {eventname}",
                    handler.GetType().FullName,
                    e.GetType().FullName);
            }

            if (ce is not null && ce.IsCancelled)
                return;
        }

    }

    public List<BeaconCommand> GetRegisteredCommands() =>
        _pluginServiceProvider == null ? new() : _pluginServiceProvider.GetServices<BeaconCommand>().ToList();
}
