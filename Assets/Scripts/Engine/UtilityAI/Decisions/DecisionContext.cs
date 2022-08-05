using System;
using Unity.Entities;

namespace UtilityAI
{
    public struct DecisionContext : IEquatable<DecisionContext>
    {
        public short DSEId;
        public Entity Decision, Target;

        public byte Override;

        public DecisionContext(short dseId, bool force, Entity decision, Entity targetId)
        {
            DSEId = dseId;
            Override = force ? (byte)1 : (byte)0;
            Decision = decision;
            Target = targetId;
        }


        public bool Equals (DecisionContext other)
        {
            return (Decision == other.Decision) && (DSEId == other.DSEId) && (Target == other.Target);
        }

        public DecisionHistory GetDecisionHistory ()
        {
            return new DecisionHistory(DSEId, Decision, Target);
        }

        public override int GetHashCode ()
        {
            return new DecisionHistory(DSEId, Decision, Target).GetHashCode();
        }

        public override string ToString () => $"{DSEId} #{Target.Index}";
    }
}