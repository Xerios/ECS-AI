using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [AlwaysUpdateSystem]
    [UpdateAfter(typeof(UtilityAILifeCycleBarrier))]
    public class ConsiderationDestroyDeadSystem : JobComponentSystem
    {
        UtilityAILifeCycleAfterDestroyBarrier barrier;

        protected override void OnCreateManager ()
        {
            barrier = World.GetExistingSystem<UtilityAILifeCycleAfterDestroyBarrier>();
        }

        [BurstCompile]
        struct RemoveDeadConsidJob : IJobForEachWithEntity<ConsiderationDecisionParent>
        {
            [ReadOnly] public EntityCommandBuffer.Concurrent Commands;
            [ReadOnly] public ComponentDataFromEntity<DecisionId> decisionIds;

            public void Execute (Entity entity, int index, [ReadOnly] ref ConsiderationDecisionParent parent)
            {
                if (!decisionIds.Exists(parent.Value)) {
                    // Debug.Log("Consideration destroy: " + parent.Value + " > " + entity);
                    Commands.DestroyEntity(index, entity);
                }
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var cmds = barrier.CreateCommandBuffer();

            var job = new RemoveDeadConsidJob
            {
                Commands = cmds.ToConcurrent(),
                decisionIds = GetComponentDataFromEntity<DecisionId>(true)
            }.Schedule(this, inputDeps);

            job.Complete();

            return job;
        }
    }
}