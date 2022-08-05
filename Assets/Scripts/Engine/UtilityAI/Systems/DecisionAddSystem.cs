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
    public class DecisionAddSystem : JobComponentSystem
    {
        UtilityAILifeCycleBarrier barrier;
        // EntityQuery added;

        protected override void OnCreateManager ()
        {
            barrier = World.GetExistingSystem<UtilityAILifeCycleBarrier>();
            // added = GetEntityQuery(
            //         ComponentType.ReadOnly(typeof(DecisionToAdd)),
            //         ComponentType.ReadOnly(typeof(DecisionId)),
            //         ComponentType.ReadOnly(typeof(DecisionTarget)),
            //         ComponentType.ReadOnly(typeof(DecisionSelfEntity)),
            //         ComponentType.Exclude(typeof(DecisionForget))
            //         );
        }

        [BurstCompile]
        [RequireComponentTag(typeof(DecisionToAdd))]
        [ExcludeComponent(typeof(DecisionForget))]
        struct AddDecisionJob : IJobForEachWithEntity<DecisionId, DecisionTarget, DecisionMindEntity>
        {
            public BufferFromEntity<DecisionInternal> decInternalFromEntity;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionId dseId, [ReadOnly] ref DecisionTarget target, [ReadOnly] ref DecisionMindEntity self)
            {
                var buffer = decInternalFromEntity[self.Value];

                for (int j = 0; j != buffer.Length; j++) {
                    if (buffer[j].dseId == dseId.Id && buffer[j].target == target.Id) {
                        buffer[j] = buffer[j].With(entity);
                        // Debug.Log($"Add decision: {self.Value} > {entity} ({buffer.Length})");
                        break;
                    }
                }

                // string debug = $"<b>AddDecisionJob {self.Value}: </b>( NEW {entity} ): ({buffer.Length}): ";

                // for (int i = 0; i != buffer.Length; i++) debug += buffer[i].decisionEntity + ", ";
                // Debug.Log(debug);
            }
        }

        [RequireComponentTag(typeof(DecisionToAdd))]
        [ExcludeComponent(typeof(DecisionForget))]
        struct AddConsiderationsJob : IJobForEachWithEntity<DecisionId, DecisionTarget, DecisionSelfEntity, DecisionMindEntity>
        {
            public EntityCommandBuffer.Concurrent cmds;
            [ReadOnly] public NativeHashMap<short, int> considerationCount;
            [ReadOnly] public NativeMultiHashMap<short, ConsiderationParams> considerations;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionId dseId, [ReadOnly] ref DecisionTarget targetz, [ReadOnly] ref DecisionSelfEntity self,
                [ReadOnly] ref DecisionMindEntity mind)
            {
                var target = targetz.Id;

                int considCount;

                considerationCount.TryGetValue(dseId.Id, out considCount);

                ConsiderationParams considerationParams;

                float consdierationModFactor = 1f - (1f / considCount);
                // Debug.Log($"Add decision2: {mind.Value} > {entity}");

                NativeMultiHashMapIterator<short> it;
                for (bool success = considerations.TryGetFirstValue(dseId.Id, out considerationParams, out it);
                    success; success = considerations.TryGetNextValue(out considerationParams, ref it)) {
                    DecisionContext context = new DecisionContext(dseId.Id, false, entity, target);

                    var newEntity = cmds.CreateEntity(index, UtilityAIArchetypes.ConsiderationArchetype);
                    cmds.SetSharedComponent(index, newEntity, new ConsiderationType { DataType = considerationParams.DataType });
                    cmds.SetComponent(index, newEntity, new ConsiderationDecisionParent { Value = entity });
                    cmds.SetComponent(index, newEntity, new ConsiderationMindParent { Value = mind.Value });
                    cmds.SetComponent(index, newEntity, new ConsiderationModfactor { Value = consdierationModFactor });
                    cmds.SetComponent(index, newEntity, new ConsiderationCurve { UtilCurve = considerationParams.UtilCurve });
                    cmds.SetComponent(index, newEntity, new ConsiderationData
                        {
                            Value = considerationParams.Value,
                            dseId = dseId.Id,
                            Self = self.Value,
                            Target = target
                        });
                    // cmds.SetComponent(index, newEntity, new ConsiderationScore { Value = 0 });
                }
            }
        }

        [RequireComponentTag(typeof(DecisionToAdd))]
        [ExcludeComponent(typeof(DecisionForget))]
        struct AddDecisionRemoveTagJob : IJobForEachWithEntity<DecisionTarget>
        {
            public EntityCommandBuffer.Concurrent cmds;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionTarget target)
            {
                cmds.RemoveComponent<DecisionToAdd>(index, entity);
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var cmds = barrier.CreateCommandBuffer();

            var changeInternalDecisionsJob = new AddDecisionJob {
                decInternalFromEntity = GetBufferFromEntity<DecisionInternal>(false),
            }.ScheduleSingle(this, inputDeps);

            var clearAddTagJob = new AddDecisionRemoveTagJob {
                cmds = cmds.ToConcurrent(),
            }.Schedule(this, changeInternalDecisionsJob);

            var decisions = UtilityAILoop.Instance.calculateDecisionsSystem.considerations;

            var addConsiderationsJob = new AddConsiderationsJob {
                cmds = barrier.CreateCommandBuffer().ToConcurrent(),
                considerationCount = UtilityAILoop.Instance.calculateDecisionsSystem.considerationCount,
                considerations = UtilityAILoop.Instance.calculateDecisionsSystem.considerations,
            }.Schedule(this, clearAddTagJob);

            barrier.AddJobHandleForProducer(addConsiderationsJob);

            return addConsiderationsJob;
        }
    }
}