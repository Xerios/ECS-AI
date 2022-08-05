using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    public struct ActionManagerSelf : ISharedComponentData, IEquatable<ActionManagerSelf>
    {
        public ActionManager Value;

        public bool Equals (ActionManagerSelf other) => Value == other.Value;
        public override int GetHashCode () => Value.GetHashCode();
    }
}