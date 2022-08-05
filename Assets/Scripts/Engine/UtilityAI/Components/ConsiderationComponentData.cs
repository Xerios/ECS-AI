using System;
using Unity.Entities;

namespace UtilityAI
{
    public struct ConsiderationType : ISharedComponentData, IEquatable<ConsiderationType>
    {
        public ConsiderationMap.Types DataType;

        public bool Equals (ConsiderationType other) => DataType == other.DataType;
        public override int GetHashCode () => DataType.GetHashCode();
    }


    public struct ConsiderationDecisionParent : IComponentData
    {
        public Entity Value;
    }

    public struct ConsiderationMindParent : IComponentData
    {
        public Entity Value;
    }

    public struct ConsiderationScore : IComponentData
    {
        public float Value;
    }

    public struct ConsiderationData : IComponentData
    {
        public ConsiderationParams.UnionValue Value;
        public short dseId;
        public Entity Self, Target;
    }

    public struct ConsiderationCurve : IComponentData
    {
        public Curve UtilCurve;
    }

    public struct ConsiderationModfactor : IComponentData
    {
        public float Value;
    }
}