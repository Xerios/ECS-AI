using System;
using Unity.Entities;

namespace UtilityAI
{
    [InternalBufferCapacity(32)]
    public struct DecisionOption : IBufferElementData, IEquatable<DecisionOption>
    {
        public Entity DecisionEntity;
        public short DSEId;
        public Entity TargetId;
        public DecisionFlags Flags;
        public float Score;

        public DecisionOption(Entity decisionEnt, short dseId, Entity signalId, DecisionFlags flags, float score)
        {
            DecisionEntity = decisionEnt;
            DSEId = dseId;
            TargetId = signalId;
            Flags = flags;
            Score = score;
        }

        public bool Equals (DecisionOption other)
        {
            return (DSEId == other.DSEId) && (TargetId == other.TargetId);
        }

        public DecisionHistory ToDecisionHistory ()
        {
            return new DecisionHistory(DSEId, DecisionEntity, TargetId);
        }

        public DecisionContext GetContext ()
        {
            return new DecisionContext(DSEId, (Flags & DecisionFlags.OVERRIDE) != 0, DecisionEntity, TargetId);
        }

        public override int GetHashCode ()
        {
            int hbsh = 17;

            hbsh = hbsh * 23 * DSEId;
            hbsh = hbsh * 23 * TargetId.Index;
            return hbsh;
        }

        public override string ToString () => $"{DSEId} {TargetId}";
    }
}