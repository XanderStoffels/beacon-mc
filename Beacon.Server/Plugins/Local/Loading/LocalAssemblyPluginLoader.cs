using Beacon.API.Plugins;
using Beacon.PluginEngine;
using Beacon.Server.Plugins;
using Beacon.Server.Plugins.Local;
using Microsoft.Extensions.Logging;

namespace Beacon.Server.Plugins.Local.Loading
{
    public class LocalAssemblyPluginLoader : IPluginLoader
    {
        private readonly ILogger<LocalAssemblyPluginLoader> _logger;
        private readonly DirectoryInfo _pluginFolder;

        public LocalAssemblyPluginLoader(ILogger<LocalAssemblyPluginLoader> logger)
        {
            _logger = logger;
            _pluginFolder = new("plugins");

        }

        public ValueTask<List<IPluginContainer>> LoadAsync(CancellationToken cToken = default)
        {
            // Create folder if it does not exist.
            _pluginFolder.Create();

            var files = _pluginFolder.GetFiles("*.dll");
            var loadedPlugins = new List<IPluginContainer>(files.Length);

            foreach (var file in files)
            {
                var context = CreateContextFromFile(file);
                // No warning log needed here, problems are logged in CreateContextFromFile.
                if (context == null) continue;
                loadedPlugins.Add(context);
                _logger.LogInformation("Discovered plugin {pluginname} v{version}", context.Plugin?.Name, context.Plugin?.Version.ToString());
            }

            return ValueTask.FromResult(loadedPlugins);
        }

        private PluginAssemblyContainer? CreateContextFromFile(FileInfo file)
        {
            var loadContext = new PluginAssemblyLoadContext(file);
            loadContext.LoadFromAssemblyPath(file.FullName);

            var assembly = loadContext.LoadFromAssemblyPath(file.FullName);
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

            try
            {
                if (Activator.CreateInstance(pluginType) is IBeaconPlugin plugin)
                    return new PluginAssemblyContainer(plugin, loadContext);

                _logger.LogWarning("Could not create an instance of plugin from assembly {filename}!", file.Name);
                return null;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Could not create an instance of plugin from assembly {filename}!", file.Name);
                return null;
            }
        }
    }
}
