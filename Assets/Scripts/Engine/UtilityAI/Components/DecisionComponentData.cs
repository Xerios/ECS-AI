using Unity.Entities;

namespace UtilityAI
{
    public struct DecisionSelfEntity : IComponentData
    {
        public Entity Value;
    }
    public struct DecisionMindEntity : IComponentData
    {
        public Entity Value;
    }

    public struct DecisionId : IComponentData
    {
        public short Id;
    }

    public struct DecisionTarget : IComponentData
    {
        public Entity Id;
        public DecisionFlags Flags;
    }

    public struct DecisionWeight : IComponentData
    {
        public float Value;
    }

    public struct DecisionPreferred : IComponentData
    {
        public float Value;
    }

    public struct DecisionFailed : IComponentData
    {
        public float Value;
    }

    public struct DecisionLastSeen : IComponentData
    {
        public float Value;
    }

    public struct DecisionScore : IComponentData
    {
        public float Value;
    }

    /*public struct DecisionUsage : IComponentData
       {
        public long Value;
       }*/

    public struct DecisionToAdd : IComponentData
    {}

    public struct DecisionForget : IComponentData
    {}

    public struct DecisionNoTarget : IComponentData
    {}

    [InternalBufferCapacity(20)]
    public struct DecisionHistoryRecord : IBufferElementData
    {
        public Entity target;
        public short dseId;
        public float StartTime, EndTime;

        public float Duration => EndTime - StartTime;
    }

    [InternalBufferCapacity(8)]
    public struct AddNewTargets : IBufferElementData
    {
        public Entity sender;
        public uint decisionTags;
        public byte decisionFlags;

        public AddNewTargets(Entity entity2, uint decisionTags, byte flags) : this()
        {
            this.sender = entity2;
            this.decisionTags = decisionTags;
            this.decisionFlags = flags;
        }
        public AddNewTargets(Entity entity2, uint decisionTags, DecisionFlags flags) : this()
        {
            this.sender = entity2;
            this.decisionTags = decisionTags;
            this.decisionFlags = (byte)flags;
        }
    }

    [InternalBufferCapacity(8)]
    public struct DecisionInternal : IBufferElementData
    {
        public short dseId;
        public Entity decisionEntity;
        public Entity target;

        public DecisionInternal(short dSEId, Entity decisionEntity, Entity target)
        {
            this.dseId = dSEId;
            this.decisionEntity = decisionEntity;
            this.target = target;
        }

        public DecisionInternal With (Entity newDecisionEntity) => new DecisionInternal(this.dseId, newDecisionEntity, target);
    }
    // [InternalBufferCapacity(8)]
    // public struct DecisionInternalTargets : IBufferElementData
    // {
    //     public short dseId;
    //     // public Entity decision;
    //     public Entity target0, target1, target2, target3, target4, target5, target6, target7;

    //     public DecisionInternalTargets(short dSEId)
    //     {
    //         this.dseId = dSEId;
    //         // this.decision = decision;
    //         this.target0 = Entity.Null;
    //         this.target1 = Entity.Null;
    //         this.target2 = Entity.Null;
    //         this.target3 = Entity.Null;
    //         this.target4 = Entity.Null;
    //         this.target5 = Entity.Null;
    //         this.target6 = Entity.Null;
    //         this.target7 = Entity.Null;
    //     }


    //     public DecisionInternalTargets(short dSEId, Entity target)
    //     {
    //         this.dseId = dSEId;
    //         // this.decision = decision;
    //         this.target0 = target;
    //         this.target1 = Entity.Null;
    //         this.target2 = Entity.Null;
    //         this.target3 = Entity.Null;
    //         this.target4 = Entity.Null;
    //         this.target5 = Entity.Null;
    //         this.target6 = Entity.Null;
    //         this.target7 = Entity.Null;
    //     }

    //     public DecisionInternalTargets Add (Entity target)
    //     {
    //         if (target0.Equals(Entity.Null)) {
    //             this.target0 = target;
    //             return this;
    //         }else if (target1.Equals(Entity.Null)) {
    //             this.target1 = target;
    //             return this;
    //         }else if (target2.Equals(Entity.Null)) {
    //             this.target2 = target;
    //             return this;
    //         }else if (target3.Equals(Entity.Null)) {
    //             this.target3 = target;
    //             return this;
    //         }else if (target4.Equals(Entity.Null)) {
    //             this.target4 = target;
    //             return this;
    //         }else if (target6.Equals(Entity.Null)) {
    //             this.target6 = target;
    //             return this;
    //         }else if (target7.Equals(Entity.Null)) {
    //             this.target7 = target;
    //             return this;
    //         }
    //         return default;
    //     }

    //     public DecisionInternalTargets Remove (Entity target)
    //     {
    //         if (target0.Equals(target)) {
    //             this.target0 = Entity.Null;
    //             return this;
    //         }else if (target1.Equals(target)) {
    //             this.target1 = Entity.Null;
    //             return this;
    //         }else if (target2.Equals(target)) {
    //             this.target2 = Entity.Null;
    //             return this;
    //         }else if (target3.Equals(target)) {
    //             this.target3 = Entity.Null;
    //             return this;
    //         }else if (target4.Equals(target)) {
    //             this.target4 = Entity.Null;
    //             return this;
    //         }else if (target6.Equals(target)) {
    //             this.target6 = Entity.Null;
    //             return this;
    //         }else if (target7.Equals(target)) {
    //             this.target7 = Entity.Null;
    //             return this;
    //         }
    //         return default;
    //     }

    //     public bool Contains (Entity target)
    //     {
    //         if (target0.Equals(target)) return true;
    //         else if (target1.Equals(target)) return true;
    //         else if (target2.Equals(target)) return true;
    //         else if (target3.Equals(target)) return true;
    //         else if (target4.Equals(target)) return true;
    //         else if (target6.Equals(target)) return true;
    //         else if (target7.Equals(target)) return true;
    //         return false;
    //     }
    // }
}