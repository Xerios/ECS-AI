using Engine;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace UtilityAI
{
    public struct AgentSelf : ISharedComponentData, IEquatable<AgentSelf>
    {
        public Agent Value;

        public bool Equals (AgentSelf other) => Value == other.Value;
        public override int GetHashCode () => Value.GetHashCode();
    }

    public struct ScoreEvaluate : ISharedComponentData, IEquatable<ScoreEvaluate>
    {
        public Func<Entity, float> Evaluate;

        public bool Equals (ScoreEvaluate other) => Evaluate == other.Evaluate;
        public override int GetHashCode () => Evaluate.GetHashCode();
    }

    public struct AccessTagData : IComponentData
    {
        public uint Value;
    }
}