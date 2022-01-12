using Beacon.Server;
using Beacon.Server.States;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Reflection;
using Beacon.Server.Plugins;
using Beacon.Server.Plugins.Local.Loading;


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

// Setup host and start server.
try
{
    await Host.CreateDefaultBuilder()
        .UseSerilog()
        .ConfigureServices((builder, services) =>
        {
            // Add internal connection states.
            services.AddTransient<HandshakeState>();
            services.AddTransient<StatusState>();

            services.AddSingleton<IPluginController, PluginController>();
            services.AddTransient<IPluginLoader, LocalAssemblyPluginLoader>();
            services.AddSingleton<BeaconServer>();
  
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
