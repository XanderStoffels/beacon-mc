using Microsoft.Extensions.ObjectPool;

namespace Beacon.Server.Net.Packets
{
    internal interface IPooled 
    {
        /// <summary>
        /// Fill in the pooled objects properties from a given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cToken"></param>
        /// <returns></returns>
        ValueTask HydrateAsync(Stream stream, CancellationToken cToken = default);
       
    }
}
