using Beacon.API.Models;
using Beacon.API.Worlds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API.Entities
{
    public interface IEntity
    {
        public IWorld World { get; }
        public ValueTask TeleportTo(Location location);
        public ValueTask Destroy();
    }
}
