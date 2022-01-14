using Beacon.Server;
using Beacon.Server.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;
using Beacon.Server.Plugins;
using Beacon.Server.Plugins.Local.Loading;
using Beacon.Server.Net;
using Beacon.API.Events;
using Beacon.API;


// Show logo.
var serverVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
var mcVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<MincraftVersionAttribute>()?.MinecraftVersion.ToString(3);

var logo = Resources.LOGO
    .Replace("{VERSION}", serverVersion)
    .Replace("{MCVERSION}", mcVersion);

Console.WriteLine($"\n{logo}\n");

// Setup logger.
Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {Scope}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

try
{
    await Host.CreateDefaultBuilder()
        .UseSerilog()
        .ConfigureServices((builder, services) =>
        {
            // Do not add Transient objects that are often going to be instantiated (like packets). They will never get collected by the GC and cause
            // memory leaks because they are kept in scope by Microsoft's DI implementation. Consider using objectpools instead.
            services.AddSingleton<BeaconServer>();
            services.AddSingleton<PluginController>();

            services.AddSingleton<IServer>(provider => provider.GetRequiredService<BeaconServer>());
            services.AddSingleton<IMinecraftEventBus>(provider => provider.GetRequiredService<PluginController>());
            services.AddSingleton<IPluginController>(provider => provider.GetRequiredService<PluginController>());
            services.AddSingleton<IPluginLoader, LocalAssemblyPluginLoader>();
            
  
            services.AddHostedService<BeaconServer>();
        })
        .Build()
        .RunAsync();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "The server crashed!");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
