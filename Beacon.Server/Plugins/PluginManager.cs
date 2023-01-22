using System.Reflection;
using Beacon.API.Events;
using Beacon.API.Plugins;
using Beacon.Server.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Plugins;

public class PluginManager : IPluginManager
{
    private IServiceProvider _pluginServices;
    private readonly Dictionary<Type, List<IMinecraftEventHandler<MinecraftEvent>>> _handlers;

    public PluginManager()
    {
        _handlers = new(0, TypeComparer.Instance);
    }

    public TService? GetService<TService>()
    {
        return _pluginServices.GetService<TService>();
    }

    public async Task FireEventAsync<TEvent>(TEvent e, CancellationToken cancelToken) where TEvent : MinecraftEvent
    {
        if (!_handlers.ContainsKey(e.GetType()))
        {            
            var handlers = _pluginServices
                .GetServices<IMinecraftEventHandler<TEvent>>()
                .OrderBy(h => h.Priority)
                .Cast<IMinecraftEventHandler<MinecraftEvent>>()
                .ToList();

            _handlers.Add(e.GetType(), handlers);
        }
        
        if (!_handlers.TryGetValue(e.GetType(), out var cachedHandlers))
            return;

        if (e is ICancellable cancellable)
            foreach (var handler in cachedHandlers.TakeWhile(_ => !cancellable.IsCanceled))
                await handler.HandleAsync(e, cancelToken);
        else
            foreach (var handler in cachedHandlers)
                await handler.HandleAsync(e, cancelToken);
    }


    public static Task<PluginManager> CreateAsync(Action<IServiceCollection> withServices)
    {
        var manager = new PluginManager();
        var services = new ServiceCollection();
        var proxy = new ServiceRegistration(services);
        
        services.AddSingleton<IPluginManager>(manager);
        withServices(services);
        
        var dir = new DirectoryInfo("plugins");
        if (!dir.Exists)dir.Create();

        var plugins = dir.GetFiles("*.dll")
            .Select(s => Assembly.LoadFrom(s.FullName))
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IPlugin).IsAssignableFrom(t))
            .Select(t =>
            {
                var plugin = (IPlugin)Activator.CreateInstance(t)!;
                plugin.ConfigureServices(proxy);
                return plugin;
            })
            .ToList();
        
    }

}

file class TypeComparer : IEqualityComparer<Type>
{
    public static TypeComparer Instance { get; } = new();

    private TypeComparer() { }

    public bool Equals(Type? x, Type? y)
    {
        return x?.FullName == y?.FullName;
    }

    public int GetHashCode(Type obj)
    {
        return obj.FullName?.GetHashCode() ?? obj.GetHashCode();
    }
}