using Engine;
using Game;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class DecisionClearDebugSystem : JobComponentSystem
    {
        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(typeof(DebugConsideration));
        }

        public int Length;

        [BurstCompile]
        public struct ClearJob : IJobChunk
        {
            public ArchetypeChunkBufferType<DebugConsideration> optionType;

            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset)
            {
                var option = chunk.GetBufferAccessor(optionType);

                for (int i = 0; i < chunk.Count; i++) {
                    option[i].Clear();
                }
            }
        }


        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            Length = m_Group.CalculateLength();

            var clearJob = new ClearJob {
                optionType = GetArchetypeChunkBufferType<DebugConsideration>()
            }.Schedule(m_Group, inputDeps);

            return clearJob;
        }
    }
}