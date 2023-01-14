using Beacon.CLI;
using Beacon.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices((builder, services) =>
    {
        services.AddBeaconServer<Startup>();
    })
    .UseSerilog((host, options) =>
    {
        options
            .MinimumLevel.Is(host.HostingEnvironment.IsProduction() ? LogEventLevel.Information : LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
    .Build()
    .RunAsync();
    
