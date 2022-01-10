using Beacon.API.Plugins;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;

namespace Beacon.Server.Plugins
{
    internal class FilePluginDiscovery : IPluginDiscovery
    {
        private readonly DirectoryInfo _pluginFolder;
        private readonly ILogger<FilePluginDiscovery> _logger;

        public FilePluginDiscovery(ILogger<FilePluginDiscovery> logger)
        {
            _logger = logger;
            _pluginFolder = new("plugins");
        }

        public List<IBeaconPlugin> DiscoverPlugins(CancellationToken cToken = default)
        {
            var plugins = new List<IBeaconPlugin>();
            _pluginFolder.Create();

            foreach (var file in _pluginFolder.GetFiles("*.dll"))
            {
                if (file.Length == 0) continue;
                try
                {
                    var plugin = LoadPluginFromFile(file);
                    if (plugin == null) continue;
                    _logger.LogInformation("Discovered plugin {pluginname} v{version}", plugin.Name, plugin.Version.ToString());
                    plugins.Add(plugin);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error while loading plugin from assembly {filename}. Is it a valid assembly?", file.Name);
                }
            }
            return plugins;
        }

        private IBeaconPlugin? LoadPluginFromFile(FileInfo file)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file.FullName);
            var pluginType = assembly
                .GetTypes()
                .FirstOrDefault(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IBeaconPlugin)));

            if (pluginType == null)
            {
                _logger.LogWarning("{filename} assembly does not contain a plugin!", file.Name);
                return null;
            }

            if (pluginType.GetConstructor(Type.EmptyTypes) == null)
            {
                _logger.LogWarning("No parameterless constructor found for plugin in assembly {filename}!", file.Name);
                return null;
            }

            var plugin = Activator.CreateInstance(pluginType) as IBeaconPlugin;
            if (plugin == null)
                _logger.LogWarning("Could not create an instance of plugin from assembly {filename}!", file.Name);

            return plugin;
        }
    }
}
