using Beacon.ConsoleApp;
using Beacon.Server;
using Beacon.Server.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

var serverVersion = BeaconServer.Version?.ToString(3);
var mcVersion = BeaconServer.McVersion.ToString(3);

Console.WriteLine(Resources.LOGO
    .Replace("{VERSION}", serverVersion ?? "Unknown")
    .Replace("{MCVERSION}", mcVersion));
Console.WriteLine();

Log.Logger = new LoggerConfiguration()
#if DEBUG
    .WriteTo.Console()
#else
    .WriteTo.Console(LogEventLevel.Information)
#endif
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder()
    //.ConfigureLogging(options => options.AddFilter("Microsoft", LogLevel.Critical))
    .UseConsoleLifetime(options => options.SuppressStatusMessages = true)
    .UseSerilog()
    .UseBeacon(options =>
    {
        if (args.Length > 0)
            options.WorkingDirectory = new DirectoryInfo(args[0]);
    });

await host.RunConsoleAsync();