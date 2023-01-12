


using Beacon.Hosting;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddBeaconServer<Startup>();
    })
    .Build()
    .RunAsync();