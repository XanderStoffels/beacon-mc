﻿using Beacon.API.Events;
using Beacon.Plugins;
using Beacon.Server.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Beacon.Server.Plugins;
internal class PluginManager : IMinecraftEventBus
{
	private readonly ILogger<PluginManager> _logger;
	private readonly List<IPluginLoader> _loaders;
	private readonly ILoggerFactory _loggerFactory;
	private readonly List<PluginContainer> _containers;
	public PluginManager(ILogger<PluginManager> logger, IServiceProvider provider)
	{
        _logger = logger;
		_loaders = provider.GetServices<IPluginLoader>().ToList();
		_loggerFactory = provider.GetRequiredService<ILoggerFactory>();
		_containers = new();
	}
    
	public async Task LoadPlugins()
	{
        if (_containers.Any()) 
			throw new InvalidOperationException("There are currently plugins loaded. Please unload those first.");
		
		var pluginSerivcesLookup = new Dictionary<PluginContainer, IServiceCollection>();
		var publicServices = new ServiceCollection();

        if (_loaders.Count == 0)
        {
            _logger.LogWarning("No plugin loaders found. Plugins will not be loaded.");
            return;
        }

        // Configure the public and local services for each plugin.
        foreach (var loader in _loaders) 
		{
			var containers = await loader.LoadAsync();		
			foreach (var container in containers)
			{
				var localServices = new ServiceCollection()
					.AddSingleton(f => _loggerFactory.CreateLogger(container.PluginName));
                
                pluginSerivcesLookup[container] = localServices;
                container.ConfigureServices(publicServices, localServices);
			}    
		}

        // After all public services have been added to the same collection, create a service store for each plugin container.
		// Share the same instances of public services for each plugin.
        var publicProvider = publicServices.BuildServiceProvider();
		foreach (var container in pluginSerivcesLookup.Keys)
		{
			var localServices = pluginSerivcesLookup[container];
			var store = new ServiceStore(localServices, publicProvider);
			container.SetServiceStore(store);
			await container.EnableAsync();
            _containers.Add(container);
			_logger.LogInformation("Enabled plugin {name}", container.PluginName);
        }  
	}

    public async Task FireEventAsync<TEvent>(TEvent e, CancellationToken cancelToken = default) where TEvent : MinecraftEvent
    {
		var watch = Stopwatch.StartNew();

		foreach (var task in _containers.Select(container => container.HandleEventAsync<TEvent>(e, cancelToken)))
		{
			await task;
		}
		watch.Stop();
		_logger.LogInformation("Event {event} took {time}ms", typeof(TEvent).Name, watch.ElapsedMilliseconds);
    }

}
