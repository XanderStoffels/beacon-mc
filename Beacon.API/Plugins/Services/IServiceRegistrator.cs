using Beacon.API.Commands;
using Beacon.API.Events;
using Beacon.API.Events.Handling;

namespace Beacon.API.Plugins.Services;

/// <summary>
///     A service that can be used to register public or private services.
/// </summary>
public interface IServiceRegistrator
{
    public IServiceRegistrator RegisterLocal<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    public IServiceRegistrator RegisterPublic<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;

    // Regsiter a command.
    public IServiceRegistrator RegisterCommand<TCommand>()
        where TCommand : BeaconCommand;


    public IServiceRegistrator RegisterEventHandler<TEvent, TEventHandler>()
        where TEvent : MinecraftEvent
        where TEventHandler : class, IMinecraftEventHandler<TEvent>;
}