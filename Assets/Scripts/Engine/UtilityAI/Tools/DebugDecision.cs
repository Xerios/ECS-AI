using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    public struct DebugDecision : IEquatable<DebugDecision>
    {
        public readonly short DSEId;
        public readonly Entity Target;
        public readonly DecisionFlags Flags;
        public readonly float Score;
        public readonly float Bonus;
        public readonly float Mod;


        public DebugDecision(short dseId, Entity target, DecisionFlags flags, float bonus, float mod, float score)
        {
            Score = score;
            Target = target;
            DSEId = dseId;
            Bonus = bonus;
            Mod = mod;
            Flags = flags;
        }

        public static bool operator == (DebugDecision a, DebugDecision b)
        {
            return a.DSEId == b.DSEId && a.Target == b.Target;
        }

        public static bool operator != (DebugDecision a, DebugDecision b)
        {
            return a.DSEId != b.DSEId || a.Target != b.Target;
        }

        public override bool Equals (System.Object obj)
        {
            return this == (DebugDecision)obj;
        }

        public bool Equals (DebugDecision other)
        {
            return this == other;
        }

        public bool Equals (DecisionContext other)
        {
            return this.DSEId == other.DSEId && this.Target == other.Target;
        }

        public override int GetHashCode ()
        {
            return new DecisionHistory(this.DSEId, Entity.Null, this.Target).GetHashCode();
        }

        public override string ToString () => $"{Score.ToString("F2")} - {DSEId} #{Target.Index} {(Bonus != 1f ? Bonus.ToString("F3") : "")}";
    }
}