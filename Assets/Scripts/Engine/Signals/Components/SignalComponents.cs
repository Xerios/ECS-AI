using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    public struct SignalFlagsType : IComponentData
    {
        public DecisionFlags Flags;
    }
    public struct SignalActionType : IComponentData
    {
        public uint decisionTags;
    }
    public struct SignalPosition : IComponentData
    {
        public Vector3 Value;
    }

    public struct SignalAction : ISharedComponentData, IEquatable<SignalAction>
    {
        public Func<StateScript> data;

        public bool Equals (SignalAction other) => data == other.data;
        public override int GetHashCode () => data.GetHashCode();
    }

    public struct SignalGameObject : ISharedComponentData, IEquatable<SignalGameObject>
    {
        public GameObject Value;

        public bool Equals (SignalGameObject other) => Value == other.Value;
        public override int GetHashCode () => Value.GetHashCode();
    }

    public struct SignalBroadcast : IComponentData
    {
        public float radius;
        // public int limit; // TO REMOVE???
    }

    public struct SignalBroadcastDisable : IComponentData {}


    public struct SignalCastToTargets : IComponentData
    {
        public Entity Target, Target2, Target3, Target4;
    }

    public struct SignalPair : IComponentData
    {
        public Entity First;
        public Entity Second;
    }

    public struct SignalReferences : IComponentData
    {
        public short Count;
    }


    public struct SignalDeleteOnCancel : IComponentData
    {}

    public struct SignalAssigned : IComponentData
    {
        public short JobId;
    }

    public struct SignalAbilityAssignment : IComponentData
    {
        public byte Ability;
    }


    public struct SpatialTracking : IComponentData
    {}

    public struct SpatialTrackProximity : IComponentData
    {
        public float dist;

        public SpatialTrackProximity(float distance)
        {
            dist = math.min(distance, SpatialAwarnessSystem.gridCellSize * 3);
        }
    }

    [InternalBufferCapacity(0)]
    public struct EntityWithFactionPosition : IBufferElementData
    {
        // public const int CAPACITY = 128;
        public const int CAPACITY_PER_CELL = 32;

        public Entity entity;
        public byte faction;
        public float2 position;
    }

    public struct SpatialDetectionTimer : IComponentData
    {
        public const float AGGRO_LIMIT = 1f;
        public const float DEFEND = 1.9f;
        public const float MAX_LIMIT = 2f;

        public float timer;
    }

    public struct SpatialDetectionState : IComponentData
    {
        public const byte NORMAL = 0;
        public const byte AGGRO = 1;

        public byte state;
    }
    public struct SpatialDetectionSpeed : IComponentData
    {
        public float speed;
    }
}