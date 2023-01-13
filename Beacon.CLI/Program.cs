using Beacon.CLI;
using Beacon.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

Log.Information("what");

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder, services) =>
    {
        services.AddBeaconServer<Startup>();
    })
    .UseSerilog((host, options) =>
    {
        options
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Is(host.HostingEnvironment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
    .Build()
    .RunAsync();
    
