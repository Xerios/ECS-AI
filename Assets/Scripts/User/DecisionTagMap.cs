using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityAI
{
    public enum DecisionTags: uint {
        Resource = 1,
        Weapon = 1 << 1,
        Danger = 1 << 2,
        Altar = 1 << 3,
        Agent = 1 << 4,
        Enemy = 1 << 5,
        Friendly = 1 << 6,
        Build = 1 << 7,
        House = 1 << 8,
        Trigger = 1 << 9,
        TriggerAction = 1 << 10
    }
}