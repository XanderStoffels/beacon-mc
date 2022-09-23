using Beacon.API.Plugins.Exceptions;

namespace Beacon.API.Plugins.Services;

public interface IServiceStore
{
    /// <summary>
    ///     Get a service instance that your plugin has registered locally.
    ///     If the service can not be found, null is returned.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    TService? Get<TService>();

    /// <summary>
    ///     Get a service instance that your plugin has registered locally.
    ///     Throws an <see cref="ServiceNotFoundException" /> if the service is not found.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    TService GetRequired<TService>();

    /// <summary>
    ///     Get a service instance that is registered publicly by any plugin.
    ///     If the service can not be found, null is returned.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    TService? GetPublic<TService>();

    /// <summary>
    ///     Get a service instance that is registered publicly by any plugin.
    ///     Throws an <see cref="ServiceNotFoundException" /> if the service is not found.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    /// <returns></returns>
    TService GetPublicRequired<TService>();
}