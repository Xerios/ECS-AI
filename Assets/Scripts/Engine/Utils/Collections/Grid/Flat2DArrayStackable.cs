using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Engine
{
    public interface IFlat2DArrayStackable
    {
        Vector2Int GetOffset();
        float GetValue(int x, int y);
    }
}
