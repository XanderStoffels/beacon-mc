
using Beacon.ConsoleApp;
using Beacon.Server;
using Beacon.Server.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Serilog;

var serverVersion = BeaconServer.VERSION.ToString(3);
var mcVersion = BeaconServer.McVersion.ToString(3);

Console.WriteLine(Resources.LOGO
    .Replace("{VERSION}", serverVersion)
    .Replace("{MCVERSION}", mcVersion));
Console.WriteLine();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddBeacon(options =>
        {
            options.WorkingDirectory = args.FirstOrDefault() ?? Environment.CurrentDirectory;
        });
    })
    .UseSerilog()
    .ConfigureLogging(options =>
    {
        options.AddFilter("Microsoft", LogLevel.Critical);
    });

await host.RunConsoleAsync();