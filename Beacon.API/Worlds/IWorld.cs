using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API.Worlds
{
    public interface IWorld
    {
        public string Name { get; }
        public ValueTask<IBlock> GetBlockAt();
    }
}
