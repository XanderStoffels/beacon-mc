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
    .WriteTo.Console()
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder()
    .UseSerilog()
    .ConfigureLogging(options => { options.AddFilter("Microsoft", LogLevel.Critical); })
    .UseBeacon(options =>
    {
        options.ServerOptions.WorkingDirectory = args.FirstOrDefault() ?? Environment.CurrentDirectory;
    });

await host.RunConsoleAsync();