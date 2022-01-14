using Beacon.API.Plugins;
using Beacon.PluginEngine;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Beacon.Server.Plugins.Local.Loading;

public class LocalAssemblyPluginLoader : IPluginLoader
{
    private readonly ILogger<LocalAssemblyPluginLoader> _logger;
    private readonly DirectoryInfo _pluginFolder;
    private readonly Version _minVersion;
    private readonly Version _maxVersion;


    public LocalAssemblyPluginLoader(ILogger<LocalAssemblyPluginLoader> logger)
    {
        _logger = logger;
        _pluginFolder = new("plugins");
        _minVersion = AssemblyName.GetAssemblyName(typeof(IBeaconPlugin).Assembly.Location).Version ?? throw new ArgumentException("API Verison could not be determined");
        _maxVersion = _minVersion;
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
        var assembly = loadContext.LoadFromAssemblyPath(file.FullName);
        Type? pluginType = default;

        // Check if the plugin uses Beacon.API.
        var apiUsedByPlugin = assembly.GetReferencedAssemblies().FirstOrDefault(a => a.Name == "Beacon.API");
        if (apiUsedByPlugin is null)
        {
            _logger.LogWarning("Assembly {filename} does not use the Beacon API!", file.Name);
            return null;
        }

        // Check if the plugin uses a compatible API verison.
        // TODO: In the future, when the API is more mature, it should stay backwards compatible on major versions.
        // Right now, it should be the exact same version the Server implements.
        var versionMatch = apiUsedByPlugin.Version == _minVersion;
        if (!versionMatch)
        {
            _logger.LogWarning("Assembly {filename} does not use the correct Beacon API version and may crash at any moment!", file.Name);
            _logger.LogWarning("Above assembly uses API version {pversion}, but should use {min}", apiUsedByPlugin.Version, _minVersion);
        }

        // Try to compare types. Will crash if plugin uses an api version that is too old.
        try
        {
            pluginType = assembly
                .GetTypes()
                .FirstOrDefault(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IBeaconPlugin)));
        }
        catch (ReflectionTypeLoadException) when (!versionMatch)
        {
            _logger.LogError("Type missmatch while scanning assembly {assembly} for plugin! Probably because it uses a wrong Beacon API verison", file.Name);
            return null;
        }


        if (pluginType == null)
        {
            _logger.LogError("{filename} assembly does not contain a plugin!", file.Name);
            return null;
        }

        if (pluginType.GetConstructor(Type.EmptyTypes) == null)
        {
            _logger.LogError("No parameterless constructor found for plugin in assembly {filename}!", file.Name);
            return null;
        }

        try
        {
            if (Activator.CreateInstance(pluginType) is IBeaconPlugin plugin)
                return new PluginAssemblyContainer(plugin, loadContext);

            _logger.LogError("Could not create an instance of plugin from assembly {filename}!", file.Name);
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not create an instance of plugin from assembly {filename}!", file.Name);
            return null;
        }
    }
}
