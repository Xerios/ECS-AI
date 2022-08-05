using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityAI
{
    public enum AbilityTags: byte {
        Gather = 1 << 0,
        Defend = 1 << 1,
        Reclaim = 1 << 2
    }
}