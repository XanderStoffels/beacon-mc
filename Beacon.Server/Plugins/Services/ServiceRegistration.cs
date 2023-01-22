using Beacon.API.Events;
using Beacon.API.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Server.Plugins.Services;

public class ServiceRegistration : IServiceRegistration
{
    private readonly IServiceCollection _services;
    public ServiceRegistration(IServiceCollection services)
    {
        _services = services;
    }
    
    /// <inheritdoc cref="IServiceRegistration"/>
    public IServiceRegistration RegisterEventHandler<TEventHandler, TEvent>() 
        where TEventHandler : class, IMinecraftEventHandler<TEvent>
        where TEvent : MinecraftEvent
    {
        _services.AddSingleton<IMinecraftEventHandler<TEvent>, TEventHandler>();
        return this;
    }
    
    /// <inheritdoc cref="IServiceRegistration"/>
    public IServiceRegistration RegisterPluginService<TInterface, TImplementation>() where TInterface : class where TImplementation : class, TInterface
    {
        _services.AddSingleton<TInterface, TImplementation>();
        return this;
    }
}