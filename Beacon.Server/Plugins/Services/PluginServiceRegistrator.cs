using Beacon.API.Plugins.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

}