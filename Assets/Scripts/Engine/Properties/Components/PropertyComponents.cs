using Engine;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [InternalBufferCapacity(8)]
    public struct PropertyData : IBufferElementData
    {
        public uint Type;
        public int Value;

        public PropertyData(uint type, int value)
        {
            this.Type = type;
            this.Value = value;
        }
        public PropertyData(PropertyType type, int value)
        {
            this.Type = (uint)type;
            this.Value = value;
        }
    }

    public struct PropertyChangeMessage : IComponentData
    {
        public Entity Target;
        public uint Type;
        public int Value;
    }
}