using Beacon.API.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.PluginEngine
{
    public interface IPluginContainer : IAsyncDisposable
    {
        public string Name { get; }
        public IBeaconPlugin? Plugin { get; }
        public ValueTask UnloadAsync();
    }
}
