using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Hosting;

public static class DependencyInjection
{
    public static IServiceCollection AddBeaconServer(this IServiceCollection services, IBeaconStartup startup)
    {
        services.AddSingleton(startup);
        services.AddHostedService<BeaconHostingService>();
        
        return services;
    }

    public static IServiceCollection AddBeaconServer<TStartup>(this IServiceCollection services)
    where TStartup : IBeaconStartup, new()
    {
        var startup = Activator.CreateInstance<TStartup>();
        if (startup == null) 
            throw new InvalidOperationException($"Unable to create an instance of {typeof(TStartup).FullName}");
        
        return AddBeaconServer(services, startup);
    }
}