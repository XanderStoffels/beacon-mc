using Beacon.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API
{
    public interface IServer
    {
        Task StartAsync(CancellationToken cancelToken);
        ValueTask<ServerStatus> GetStatusAsync();
    }
}
