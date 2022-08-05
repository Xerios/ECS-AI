using Engine;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    public struct InitAgent : IComponentData
    {}

    public struct MindBelongsTo : IComponentData
    {
        public Entity Value;
    }

    public struct MindReference : IComponentData
    {
        public Entity Value;
    }

    public struct MindDestroyTag : IComponentData
    {}

    public struct BestDecision : IComponentData
    {
        public DecisionContext data;
    }

    public struct ActiveDecision : IComponentData
    {
        public Entity entity;
        public short dseId;
        public Entity target;
    }


    public struct AgentFaction : IComponentData
    {
        public byte LayerFlags;
    }

    [InternalBufferCapacity(10)]
    public struct AcceptedDecisions : IBufferElementData, IEquatable<AcceptedDecisions>
    {
        public short Id;

        public AcceptedDecisions(short id)
        {
            Id = id;
        }

        public bool Equals (AcceptedDecisions other) => Id == other.Id;
        public override int GetHashCode () => Id.GetHashCode();
    }


    public struct SelectBestDecision : IComponentData
    {}
}