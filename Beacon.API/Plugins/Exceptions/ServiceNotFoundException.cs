namespace Beacon.API.Plugins.Exceptions;

public class ServiceNotFoundException : Exception
{
    public ServiceNotFoundException(string serviceName)
        : base($"Service {serviceName} could not be found.")
    {
    }
    
}
