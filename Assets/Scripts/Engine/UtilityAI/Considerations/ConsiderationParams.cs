using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace UtilityAI
{
    [Serializable]
    public struct ConsiderationParams
    {
        public ConsiderationMap.Types DataType;

        public Curve UtilCurve;

        public UnionValue Value;

        [Serializable]
        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct UnionValue // : IEquatable<UnionValue>
        {
            [FieldOffset(0)]
            public float Min;
            [FieldOffset(4)]
            public float Max;

            [FieldOffset(0)]
            public byte Property;
            // [FieldOffset(0)]
            // public AccessTags AccessTags;

            // [FieldOffset(0)]
            // public long FullValue;

            // public bool Equals (UnionValue other)
            // {
            //     return FullValue == other.FullValue;
            // }
        }

        public ConsiderationParams(ConsiderationMap.Types name, Curve curve, byte prop, float min, float max)
        {
            DataType = name;
            UtilCurve = curve;
            Value = default;
        }

        public override string ToString ()
        {
            return DataType.ToString();
        }

#if UNITY_EDITOR
        public static ConsiderationParams New (string item)
        {
            return New((ConsiderationMap.Types)Enum.Parse(typeof(ConsiderationMap.Types), item));
        }

        public static ConsiderationParams New (ConsiderationMap.Types type)
        {
            // newConsideration.UtilityCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1, 1));
            ConsiderationParams newConsideraiton;

            ParametersType parametersType = ConsiderationMap.GetParametersType(type);

            if (parametersType.HasFlag(ParametersType.Range)) {
                newConsideraiton = new ConsiderationParams(type, new Curve(0.2f, 0f, 1f, 0f, 0f, -10f), 0, 0, 10);
            }else if (parametersType.HasFlag(ParametersType.None) || parametersType.HasFlag(ParametersType.Boolean)) {
                newConsideraiton = new ConsiderationParams(type, GetBoolean(true), 0, 0, 0);
            }else{
                newConsideraiton = new ConsiderationParams(type, new Curve(0f, 0f, 0f, 1f, 0f, 0f), 0, 0, 0);
            }
            return newConsideraiton;
        }

        public static Curve GetBoolean (bool enabled)
        {
            var animCurve = enabled ? new Curve(0f, 0f, 0f, 1f, 0f, 0f) : new Curve(0f, 0f, 1f, 0f, 0f, 0f);

            return animCurve;
        }
#endif
    }
}