using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilityAILifeCycleBarrier))]
    public class DecisionRemoveSystem : JobComponentSystem
    {
        UtilityAILifeCycleBarrier barrier;
        EntityQuery group;

        protected override void OnCreateManager ()
        {
            barrier = World.GetExistingSystem<UtilityAILifeCycleBarrier>();
        }

        // [BurstCompile]
        [RequireComponentTag(typeof(DecisionForget))]
        struct RemoveDecisionJob : IJobForEachWithEntity<DecisionId, DecisionTarget, DecisionMindEntity>
        {
            public BufferFromEntity<DecisionInternal> decInternalFromEntity;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionId dseId, [ReadOnly] ref DecisionTarget target, [ReadOnly] ref DecisionMindEntity self)
            {
                var buffer = decInternalFromEntity[self.Value];

                // if (!decisionIds.Exists(parent.Value)) {
                //     Debug.Log("Consideration destroy: " + parent.Value);
                //     Commands.DestroyEntity(index, entity);
                // }
                // #################################################################################
                //                           @TODO: COULD CAUSE ISSUES????
                //          What if decisionEntity isn't set on time? Will this be an issues?
                // #################################################################################

                for (int j = buffer.Length - 1; j >= 0; j--) {
                    if (buffer[j].dseId == dseId.Id && buffer[j].target == target.Id) {
                        buffer.RemoveAt(j);
                        break;
                    }
                }

                // string debug = $"<b>RemoveDecisionJob {self.Value}:</b> ( DEL {entity} ): ({buffer.Length}): ";

                // for (int i = 0; i != buffer.Length; i++) debug += buffer[i].decisionEntity + ", ";
                // Debug.Log(debug);
            }
        }

        [RequireComponentTag(typeof(DecisionForget))]
        [ExcludeComponent(typeof(DecisionNoTarget))]
        struct ReduceReferenceCountJob : IJobForEachWithEntity<DecisionTarget>
        {
            public EntityCommandBuffer.Concurrent cmds;
            public ComponentDataFromEntity<SignalReferences> signalReferences;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionTarget target)
            {
                // Reduce reference count ( if there's a target )
                if (target.Id != Entity.Null) {
                    if (signalReferences.Exists(target.Id)) {
                        var refCount = signalReferences[target.Id].Count;
                        cmds.SetComponent(index, target.Id, new SignalReferences { Count = (short)(refCount - 1) });
                    }
                }
            }
        }

        [RequireComponentTag(typeof(DecisionForget))]
        struct DestroyDecisionJob : IJobForEachWithEntity<DecisionMindEntity>
        {
            public EntityCommandBuffer.Concurrent cmds;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionMindEntity mind)
            {
                // Debug.Log($"Decision destroy decision2: {mind.Value} > {entity}");
                // Debug.Log($"Decision destroy: {entity}");
                // Destroy
                cmds.DestroyEntity(index, entity);
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var cmds = barrier.CreateCommandBuffer().ToConcurrent();

            var changeInternalDecisions = new RemoveDecisionJob {
                decInternalFromEntity = GetBufferFromEntity<DecisionInternal>(false),
            }.ScheduleSingle(this, inputDeps);

            var reduceRefsJob = new ReduceReferenceCountJob {
                cmds = cmds,
                signalReferences = GetComponentDataFromEntity<SignalReferences>(false),
            }.ScheduleSingle(this, changeInternalDecisions);

            var destroyJob = new DestroyDecisionJob {
                cmds = cmds,
            }.Schedule(this, reduceRefsJob);

            barrier.AddJobHandleForProducer(JobHandle.CombineDependencies(changeInternalDecisions, reduceRefsJob, destroyJob));

            return destroyJob;
        }
    }
}