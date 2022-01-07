using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beacon.API.Events
{
    public abstract class MinecraftEvent
    {
        public bool IsCancelled { get; set; }
    }
}
