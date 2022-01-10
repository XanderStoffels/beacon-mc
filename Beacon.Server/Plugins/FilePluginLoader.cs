using Beacon.API.Plugins;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;

namespace Beacon.Server.Plugins
{
    internal class FilePluginLoader : IPluginLoader
    {
        private readonly DirectoryInfo _pluginFolder;
        private readonly ILogger<FilePluginLoader> _logger;

        public FilePluginLoader(ILogger<FilePluginLoader> logger)
        {
            _logger = logger;
            _pluginFolder = new("plugins");
        }

        public ValueTask<List<IPluginContext>> LoadPluginContexts(CancellationToken cToken = default)
        {
            var contexts = new List<IPluginContext>();
            _pluginFolder.Create();

            foreach (var file in _pluginFolder.GetFiles("*.dll"))
            {
                if (file.Length == 0) continue;
                try
                {
                    var context = CreateContextFromFile(file);
                    if (context == null) continue;

                    _logger.LogInformation("Discovered plugin {pluginname} v{version}", context.Plugin.Name, context.Plugin.Version.ToString());
                    contexts.Add(context);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error while loading plugin from assembly {filename}. Is it a valid assembly?", file.Name);
                }
            }
            return ValueTask.FromResult(contexts);
        }

        private IPluginContext? CreateContextFromFile(FileInfo file)
        {
            var loadContext = new AssemblyLoadContext(file.Name, isCollectible: true);
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
                    return new PluginContext(plugin, loadContext);

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
