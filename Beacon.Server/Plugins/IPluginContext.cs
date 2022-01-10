using Beacon.API.Plugins;
using System.Runtime.Loader;

namespace Beacon.Server.Plugins
{
    internal interface IPluginContext : IAsyncDisposable
    {
        AssemblyLoadContext AssemblyContext { get; }
        IBeaconPlugin Plugin { get; }
    }
}