using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(DecisionWeightCalculateSystem))]
    public class DecisionFailedSystem : JobComponentSystem
    {
        private UtilityAIGroup1 systemGroup;

        protected override void OnCreateManager ()
        {
            systemGroup = World.GetExistingSystem<UtilityAIGroup1>();
        }

        // [BurstCompile]
        public struct FailedDecJob : IJobForEachWithEntity<DecisionFailed>
        {
            public float time;

            public EntityCommandBuffer.Concurrent cmds;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionFailed failed)
            {
                if (time > failed.Value) {
                    cmds.RemoveComponent<DecisionFailed>(index, entity);
                }
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var job = new FailedDecJob {
                cmds = systemGroup.PostUpdateCommands.ToConcurrent(),
                time = UnityEngine.Time.time,
            }.Schedule(this, inputDeps);

            job.Complete();

            return job;
        }
    }
}