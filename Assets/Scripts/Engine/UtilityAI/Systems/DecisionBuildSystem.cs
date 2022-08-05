using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class DecisionBuildSystem : JobComponentSystem
    {
        EntityQuery m_OptionsGroup;

        protected override void OnCreateManager ()
        {
            m_OptionsGroup = GetEntityQuery(typeof(DecisionOption));
        }

        [BurstCompile]
        public struct ClearJob : IJobChunk
        {
            public ArchetypeChunkBufferType<DecisionOption> optionType;

            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset)
            {
                var option = chunk.GetBufferAccessor(optionType);

                for (int i = 0; i < chunk.Count; i++) {
                    option[i].Clear();
                }
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(DecisionForget), typeof(DecisionFailed))]
        public struct BuildJob : IJobForEachWithEntity<DecisionMindEntity, DecisionId, DecisionTarget, DecisionScore>
        {
            public BufferFromEntity<DecisionOption> options;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionMindEntity self, [ReadOnly] ref DecisionId dseId, [ReadOnly] ref DecisionTarget target,
                [ReadOnly] ref DecisionScore score)
            {
                options[self.Value].Add(new DecisionOption(entity, dseId.Id, target.Id, target.Flags, score.Value));
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var clearJob = new ClearJob {
                optionType = GetArchetypeChunkBufferType<DecisionOption>()
            }.Schedule(m_OptionsGroup, inputDeps);

            var buildJob = new BuildJob {
                options = GetBufferFromEntity<DecisionOption>(false),
            }.ScheduleSingle(this, clearJob);

            return buildJob;
        }
    }
}