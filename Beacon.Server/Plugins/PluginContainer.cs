using System.Reflection;
using Beacon.API.Commands;
using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Beacon.API.Plugins;
using Beacon.Server.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Server.Plugins;

public sealed class PluginContainer : IAsyncDisposable
{
    private readonly List<BeaconCommand> _commands;
    private readonly Dictionary<Type, List<IMinecraftEventHandler<MinecraftEvent>>> _eventHandlers;
    private Assembly _assembly;
    private PluginLoadContext _loadContext;
    private IBeaconPlugin _plugin;


    internal PluginContainer(PluginLoadContext loadContext, Assembly assembly, IBeaconPlugin plugin)
    {
        _loadContext = loadContext;
        _assembly = assembly;
        _plugin = plugin;
        _commands = new List<BeaconCommand>();
        _eventHandlers = new Dictionary<Type, List<IMinecraftEventHandler<MinecraftEvent>>>();
    }

    internal ServiceStore? ServiceStore { get; private set; }
    internal bool IsDisposed { get; private set; }
    internal string PluginName => new(_plugin.Name.ToArray());
    internal Version PluginVersion => (Version)_plugin.Version.Clone();

    public async ValueTask DisposeAsync()
    {
        await _plugin.DisableAsync();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        _assembly = null;
        _plugin = null;
        ServiceStore = null;
        _loadContext.Unload();
        _loadContext = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        IsDisposed = true;
    }

    internal void ConfigureServices(IServiceCollection publicServices, IServiceCollection localServices)
    {
        // Add public services to the list.
        // Create a provider for the local services.
        var registrator = new PluginServiceRegistrator(localServices, publicServices);
        _plugin.ConfigureServices(registrator);
    }

    internal Task EnableAsync()
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
        if (ServiceStore is null) throw new InvalidOperationException("ServiceStore is null.");
        return _plugin.EnableAsync();
    }

    internal void SetServiceStore(ServiceStore serviceStore)
    {
        ServiceStore = serviceStore;
        _eventHandlers.Clear();
        _commands.Clear();
        _commands.AddRange(serviceStore.GetServices<BeaconCommand>());
    }

    internal async ValueTask HandleEventAsync<TEvent>(TEvent e, CancellationToken cancelToken)
        where TEvent : MinecraftEvent
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
        if (ServiceStore is null) throw new InvalidOperationException("Service store is not set.");

        var tasks = ServiceStore
            .GetServices<IMinecraftEventHandler<TEvent>>()
            .Select(h => h.HandleAsync(e, cancelToken));

        foreach (var task in tasks)
            await task;
    }
}