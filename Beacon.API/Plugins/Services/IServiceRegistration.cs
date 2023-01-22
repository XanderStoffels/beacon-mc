using Beacon.API.Events;

namespace Beacon.API.Plugins.Services;

public interface IServiceRegistration
{
    /// <summary>
    /// Register a Minecraft event handler.
    /// </summary>
    /// <typeparam name="TEventHandler"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <remarks>The handler will be registered as a singleton.</remarks>
    /// <returns></returns>
    IServiceRegistration RegisterEventHandler<TEventHandler, TEvent>()
        where TEventHandler : class, IMinecraftEventHandler<TEvent>
        where TEvent : MinecraftEvent;
    
    /// <summary>
    /// Register a Service that can be used by other plugins.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns>The service will have be registered as a singleton.</returns>
    IServiceRegistration RegisterPluginService<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface;
    
}