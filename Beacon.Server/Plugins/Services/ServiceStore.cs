using Beacon.API.Plugins.Exceptions;
using Beacon.API.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Server.Plugins.Services;

internal class ServiceStore : IServiceStore
{
    private readonly IServiceProvider _privateServices;
    private readonly IServiceProvider _publicServices;

    public ServiceStore(IServiceCollection localServices, IServiceProvider publicServices)
    {
        _privateServices = localServices.AddSingleton(this).BuildServiceProvider();
        _publicServices = publicServices;
    }

    public TService? Get<TService>()
    {
        return _privateServices.GetService<TService>();
    }

    public TService? GetPublic<TService>()
    {
        return _publicServices.GetService<TService>();
    }

    public TService GetPublicRequired<TService>()
    {
        var service = _publicServices.GetService<TService>();
        return service ?? throw new ServiceNotFoundException(typeof(TService).Name);
    }

    public TService GetRequired<TService>()
    {
        var service = _privateServices.GetService<TService>();
        return service ?? throw new ServiceNotFoundException(typeof(TService).Name);
    }

    internal IEnumerable<TService> GetServices<TService>()
    {
        return _privateServices.GetServices<TService>().ToList();
    }
}