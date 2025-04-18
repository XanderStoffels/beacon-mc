using Microsoft.Extensions.DependencyInjection;

namespace Beacon.Api.Plugins;

public interface IPlugin
{
    public string Id { get; }
    public Version Version { get; }
    public string Name { get; }
    
    void ConfigureServices(IServiceCollection services);
    void OnEnabled();
    void OnDisabled();
}