using Beacon.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API.Worlds
{
    public interface IChunk
    {
        public IWorld World { get; }
        public Location Location { get; }
        public bool IsLoaded { get; }
    }
}
