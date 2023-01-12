


using Beacon.CLI;
using Beacon.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
        services.AddBeaconServer<Startup>();
    })
    .Build()
    .RunAsync();