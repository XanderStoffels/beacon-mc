using Beacon.API.Commands;
using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Beacon.API.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Server.Plugins.Services;
internal class PluginServiceRegistrator : IServiceRegistrator
{
    internal IServiceCollection LocalServiceCollection { get; private set; }
    internal IServiceCollection PublicServiceCollection { get; private set; }

    public PluginServiceRegistrator(IServiceCollection local, IServiceCollection @public)
    {
        LocalServiceCollection = local;
        PublicServiceCollection = @public;
    }

    public IServiceRegistrator RegisterLocal<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        LocalServiceCollection.AddSingleton<TService, TImplementation>();
        return this;
    }

    public IServiceRegistrator RegisterPublic<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        PublicServiceCollection.AddSingleton<TService, TImplementation>();
        return this;
    }

    public IServiceRegistrator RegisterCommand<TCommand>() where TCommand : BeaconCommand
    {
        LocalServiceCollection.AddSingleton<BeaconCommand, TCommand>();
        return this;
    }

    public IServiceRegistrator RegisterEventHandler<TEvent, TEventHandler>()
        where TEvent : MinecraftEvent
        where TEventHandler : class, IMinecraftEventHandler<TEvent>
    {
        LocalServiceCollection.AddSingleton<IMinecraftEventHandler<TEvent>, TEventHandler>();
        return this;
    }
}