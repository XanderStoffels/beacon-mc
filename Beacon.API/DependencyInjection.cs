using Beacon.API.Commands;
using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.API;

public static class DependencyInjection
{
    public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
        where TEvent : MinecraftEvent where THandler : class, IMinecraftEventHandler<TEvent>
    {
        services.AddSingleton<IMinecraftEventHandler<TEvent>, THandler>();
        return services;
    }

    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services)
        where TCommand : BeaconCommand
    {
        services.AddSingleton<BeaconCommand, TCommand>();
        return services;
    }

}
