using Beacon.API.Plugins;
using Beacon.API.Plugins.Services;
using Beacon.Server.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Beacon.Server.Plugins;

public class PluginContainer : IAsyncDisposable
{
    private PluginLoadContext _loadContext;
    private Assembly _assembly;    
    private IBeaconPlugin _plugin;

    internal IServiceStore? ServiceStore { get; set; }
    internal bool IsDisposed { get; private set; }
    internal string PluginName => new(_plugin.Name.ToArray());
    internal Version PluginVersion => (Version)_plugin.Version.Clone();

    internal PluginContainer(PluginLoadContext loadContext, Assembly assembly, IBeaconPlugin plugin)
    {
        _loadContext = loadContext;
        _assembly = assembly;
        _plugin = plugin;
    }

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
}
