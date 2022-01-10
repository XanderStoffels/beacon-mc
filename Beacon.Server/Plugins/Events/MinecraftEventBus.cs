using Beacon.API.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.Server.Plugins.Events
{
    internal class MinecraftEventBus : IMinecraftEventBus
    {

        private readonly IPluginController _pluginController;
        private readonly ILogger<MinecraftEventBus> _logger;

        public MinecraftEventBus(IPluginController pluginController, ILogger<MinecraftEventBus> logger)
        {
            _pluginController = pluginController;
            _logger = logger;
        }

        public async ValueTask<TEvent> FireEventAsync<TEvent>(TEvent e, CancellationToken cToken = default) where TEvent : MinecraftEvent
        {
            _logger.LogTrace("Firing event {eventname}", e.GetType().Name);
            foreach (var handler in _pluginController.GetPluginEventHandlers<TEvent>())
            {
                if (cToken.IsCancellationRequested)
                    e.IsCancelled = true;

                if (e.IsCancelled)
                    return e;

                await handler.HandleAsync(e, cToken);
            }
            return e;
        }
    }
}
