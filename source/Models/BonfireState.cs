using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP.Models
{
    public enum BonfireState
    {
        Unknown = -1,

        Discovered = 0,

        Unlocked = 10,

        Kindled1 = 20,

        Kindled2 = 30,

        Kindled3 = 40
    }
}
