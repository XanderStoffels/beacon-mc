using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API.Events.Handling
{
    public enum Priority 
    {
        LEAST = int.MinValue,
        LOWER = -100,
        LOW = -10,
        NORMAL = 0,
        HIGH = 10,
        HIGHER = 100,
        HIGHEST = int.MaxValue
    }
}
