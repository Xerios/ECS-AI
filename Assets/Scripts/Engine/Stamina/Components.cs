using Engine;
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace UtilityAI
{
    public struct StaminaData : IComponentData
    {
        public float Value;
        public float Max;

        public static void Add (EntityManager entMgr, Entity entity, float value)
        {
            var current = entMgr.GetComponentData<StaminaData>(entity);

            current.Change(value);
            entMgr.SetComponentData(entity, current);
        }

        public static void Add (Entity entity, float value) => Add(AIManager.Instance.mgr, entity, value);

        public static void SetMax (EntityManager entMgr, Entity entity)
        {
            var max = entMgr.GetComponentData<StaminaData>(entity).Max;

            entMgr.SetComponentData(entity, new StaminaData { Value = max, Max = max });
        }

        public static void SetMax (Entity entity) => SetMax(AIManager.Instance.mgr, entity);

        public void Change (float value)
        {
            Value = math.clamp(Value + value, 0, Max);
        }
    }

    public struct AssignmentTypeData : IComponentData
    {
        public const byte NONE = 255;

        public byte TypeId;

        public static AssignmentTypeData Empty ()
        {
            return new AssignmentTypeData {
                       TypeId = NONE
            };
        }
    }

    public struct AssignmentEntityData : IComponentData
    {
        public Entity AssignedEntity;

        public static AssignmentEntityData Empty ()
        {
            return new AssignmentEntityData {
                       AssignedEntity = Entity.Null
            };
        }
    }
}