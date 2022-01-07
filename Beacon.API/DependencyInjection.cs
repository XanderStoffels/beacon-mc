using Beacon.API.Events;
using Beacon.API.Events.Handling;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : MinecraftEvent where THandler : class, IMinecraftEventHandler<TEvent>
        {
            services.AddSingleton<IMinecraftEventHandler<TEvent>, THandler>();
            return services;
        }

    }
}
