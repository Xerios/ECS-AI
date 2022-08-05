using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityAI
{
    [Flags]
    public enum AccessTags: uint {
        Human = 1 << 0,
        Animal = 1 << 1,
        BothGenders = Human | Animal
    }
}