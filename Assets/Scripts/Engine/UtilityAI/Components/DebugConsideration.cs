using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [InternalBufferCapacity(32)]
    public struct DebugConsideration : IBufferElementData
    {
        public Entity considerationEntity, decisionEntity;
        // public ConsiderationMap.Types type;
        // public float score;
    }
}