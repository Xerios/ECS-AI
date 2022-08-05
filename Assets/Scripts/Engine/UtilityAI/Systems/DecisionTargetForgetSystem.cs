using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilityAIForgetBarrier))]
    public class DecisionTargetForgetSystem : JobComponentSystem
    {
        private UtilityAIForgetBarrier barrier;

        protected override void OnCreateManager ()
        {
            barrier = World.GetExistingSystem<UtilityAIForgetBarrier>();
        }

        // [BurstCompile]
        [ExcludeComponent(typeof(DecisionNoTarget), typeof(DecisionForget))]
        struct RemoveNoTargetJob : IJobForEachWithEntity<DecisionTarget>
        {
            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;
            [ReadOnly] public ComponentDataFromEntity<SignalPosition> entitiesWithSignals;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionTarget target)
            {
                // Debug.Log($"Target exists? ({entity.Index} => {target.Id.Index}) {entitiesWithSignals.Exists(target.Id)}");

                if (!entitiesWithSignals.Exists(target.Id)) {
                    // Debug.Log($"Target no longer exists: {target.Id}");

                    Commands.AddComponent<DecisionForget>(index, entity, new DecisionForget());
                }
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            // Debug.Log($"-DecisionTargetForgetSystem-");
            var job = new RemoveNoTargetJob
            {
                Commands = barrier.CreateCommandBuffer().ToConcurrent(),
                entitiesWithSignals = GetComponentDataFromEntity<SignalPosition>(true)
            }.Schedule(this, inputDeps);

            barrier.AddJobHandleForProducer(job);
            return job;
        }
    }
}