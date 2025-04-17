using Beacon;
using Beacon.Config;
using Beacon.Net;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<ServerConfiguration>(builder.Configuration.GetSection(ServerConfiguration.SectionName));
builder.Services.AddSingleton<ServerConfiguration>(f => f.GetService<IOptions<ServerConfiguration>>()?.Value ?? ServerConfiguration.Default);
builder.Services.AddHostedService<Server>();

var host = builder.Build();

await host.RunAsync();