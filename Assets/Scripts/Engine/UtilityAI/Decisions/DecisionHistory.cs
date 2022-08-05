using System;
using Unity.Entities;

namespace UtilityAI
{
    public struct DecisionHistory : IEquatable<DecisionHistory>
    {
        private readonly short DSEId;
        private readonly Entity Target;
        public readonly Entity DecisionEntity;

        public DecisionHistory(short dseId, Entity decisionId, Entity targetId)
        {
            DSEId = dseId;
            DecisionEntity = decisionId;
            Target = targetId;
        }

        public bool Equals (DecisionHistory other)
        {
            return (DSEId == other.DSEId) && (Target == other.Target);
        }

        public short GetDSEId ()
        {
            return DSEId;
        }

        public Entity GetSignalId ()
        {
            return Target;
        }

        public override string ToString () => $"{DSEId} #{Target.Index}";
    }
}