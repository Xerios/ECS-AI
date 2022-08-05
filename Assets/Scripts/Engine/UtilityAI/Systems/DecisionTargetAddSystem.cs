using Engine;
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
    [AlwaysUpdateSystem] // Otherwise doesn't when no considerations are in place ( prob because of m_Group)
    public class DecisionTargetAddSystem : JobComponentSystem
    {
        EntityQuery m_Group;

        [BurstCompile]
        [ExcludeComponent(typeof(InitAgent))]
        public struct FactionsFixJob : IJobForEachWithEntity<MindBelongsTo>
        {
            public BufferFromEntity<AddNewTargets> buffers;
            [ReadOnly] public ComponentDataFromEntity<AgentFaction> factions;

            public void Execute (Entity entity, int index, [ReadOnly] ref MindBelongsTo belongsTo)
            {
                var buffer = buffers[entity];

                for (int j = 0; j != buffer.Length; j++) {
                    byte layerFlags = byte.MaxValue;

                    if (factions.Exists(buffer[j].sender)) layerFlags = factions[buffer[j].sender].LayerFlags;

                    var faction = factions[belongsTo.Value].LayerFlags;
                    // string toByte (byte b) => Convert.ToString(b, 2).PadLeft(8, '0');
                    // Debug.Log($"{toByte(factionLayer)} == {toByte(myLayer)} ({Faction})");

                    if ((layerFlags & faction) != faction) {
                        if (buffer[j].decisionTags == (uint)DecisionTags.Agent) {
                            // Debug.Log($"{buffer[j].sender} <--- Enemy <--- {entity}");
                            buffer[j] = new AddNewTargets(buffer[j].sender, (uint)DecisionTags.Enemy, buffer[j].decisionFlags);
                        }
                    }
                }
            }
        }

        // [BurstCompile]
        [ExcludeComponent(typeof(InitAgent))]
        public struct AddTargetsJob : IJobForEachWithEntity<MindBelongsTo>
        {
            private const int MAX_TARGETS_PER_DECISION = 8;

            public float time;
            public EntityCommandBuffer.Concurrent cmds;
            [ReadOnly] public NativeMultiHashMap<uint, short> tag2decisions;
            [ReadOnly] public BufferFromEntity<AcceptedDecisions> acceptedDecisions;
            [ReadOnly] public BufferFromEntity<AddNewTargets> buffers;
            [NativeDisableParallelForRestriction] public BufferFromEntity<DecisionInternal> internalDecisions;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<SignalReferences> signalReferences;


            public void Execute (Entity entity, int index, [ReadOnly] ref MindBelongsTo belongsTo)
            {
                var buffer = buffers[entity];

                // Debug.Log($"--{entity.Index} AddTargetsJob {buffer.Length}");

                for (int j = 0; j != buffer.Length; j++) {
                    // agent.Value.RespondToSignal(cmds, time, buffer[j].sender, buffer[j].decisionTags, (DecisionFlags)buffer[j].decisionFlags, layerFlags);
                    // mind.Value.AddDecisionTargetOption(cmds, time, buffer[j].sender, tag, (DecisionFlags)buffer[j].decisionFlags);

                    // Debug.Log($"--{entity.Index}  Add decisionOption  target:{buffer[j].sender.Index}");

                    var tag = buffer[j].decisionTags;

                    // Apply to all DSE's that use this tag
                    short dseId;
                    NativeMultiHashMapIterator<uint> decisionsIterator;

                    if (tag2decisions.TryGetFirstValue(tag, out dseId, out decisionsIterator)) {
                        do {
                            // Check if accepted
                            if (acceptedDecisions[entity].Contains(new AcceptedDecisions(dseId))) {
                                // Add decision
                                AddDecision(index, entity, dseId, buffer[j], ref belongsTo);
                            }
                        } while (tag2decisions.TryGetNextValue(out dseId, ref decisionsIterator));
                    }
                }
            }

            void AddDecision (int index, Entity entity, short dseId, AddNewTargets target, ref MindBelongsTo belongsTo)
            {
                int count = 0;

                // Lookup current decisions
                var bufferDecisions = internalDecisions[entity];
                var decisionInternalIndex = -1;
                var decisionInternal = default(DecisionInternal);

                // Find entry
                for (int i = 0; i < bufferDecisions.Length; i++) {
                    if (bufferDecisions[i].dseId == dseId) {
                        count++;
                        if (bufferDecisions[i].target == target.sender) {
                            decisionInternalIndex = i;
                            decisionInternal = bufferDecisions[i];
                        }
                    }
                }

                var flags = target.decisionFlags;

                if (decisionInternalIndex == -1) {
                    if (flags == (byte)DecisionFlags.OVERRIDE) {
                        // Forget everything else
                        for (int i = 0; i < bufferDecisions.Length; i++) {
                            if (bufferDecisions[i].dseId == dseId) {
                                if (bufferDecisions[i].decisionEntity != Entity.Null) {
                                    cmds.AddComponent(index, bufferDecisions[i].decisionEntity, new DecisionForget());
                                }
                            }
                        }
                    }
                }

                // Do we have enough targets for this decision
                if (count >= MAX_TARGETS_PER_DECISION) return;

                if (decisionInternalIndex == -1) {
                    var newEntity = cmds.CreateEntity(index, UtilityAIArchetypes.DecisionTargetArchetype);

                    cmds.SetComponent(index, newEntity, new DecisionId { Id = dseId });
                    cmds.SetComponent(index, newEntity, new DecisionMindEntity { Value = entity });
                    cmds.SetComponent(index, newEntity, new DecisionSelfEntity { Value = belongsTo.Value });
                    // cmds.SetComponent(index, newEntity, new DecisionPreferred { Value = 0f });
                    cmds.SetComponent(index, newEntity, new DecisionTarget { Id = target.sender, Flags = (DecisionFlags)target.decisionFlags });
                    cmds.SetComponent(index, newEntity, new DecisionLastSeen { Value = time + 10f });

                    bufferDecisions.Add(new DecisionInternal(dseId, Entity.Null, target.sender));

                    if (signalReferences.Exists(target.sender)) {
                        var refCount = signalReferences[target.sender].Count;
                        cmds.SetComponent(index, target.sender, new SignalReferences { Count = (short)(math.max(0, refCount) + 1) });
                    }

                    // Debug.Log($"[{entity.Index}] Add Decision Option: {UtilityAILoop.Instance.calculateDecisionsSystem.decisions[dseId]} => ({target.sender})");
                }else if (decisionInternal.decisionEntity != Entity.Null) {
                    cmds.SetComponent(index, decisionInternal.decisionEntity, new DecisionLastSeen { Value = time + 10f });

                    // Debug.Log($"[{Entity.Index}] Update Decision Option: {UtilityAILoop.Instance.calculateDecisionsSystem.decisions[dseId]} => ({target})");
                }
            }
        }

        [BurstCompile]
        [ExcludeComponent(typeof(InitAgent))]
        public struct ClearBuffersJob : IJobForEachWithEntity<MindBelongsTo>
        {
            public BufferFromEntity<AddNewTargets> buffers;

            public void Execute (Entity entity, int index, ref MindBelongsTo c0)
            {
                buffers[entity].Clear();
            }
        }

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(AgentFaction)),
                    ComponentType.ReadOnly(typeof(AddNewTargets)),
                    ComponentType.Exclude(typeof(InitAgent))
                    );
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var time = Time.time;

            var job = new FactionsFixJob {
                buffers = GetBufferFromEntity<AddNewTargets>(false),
                factions = GetComponentDataFromEntity<AgentFaction>(true)
            }.ScheduleSingle(this);

            var cmds = new EntityCommandBuffer(Allocator.TempJob);
            var job2 = new AddTargetsJob {
                time = Time.time,
                tag2decisions = UtilityAILoop.Instance.calculateDecisionsSystem.tag2decisionIds,
                buffers = GetBufferFromEntity<AddNewTargets>(true),
                acceptedDecisions = GetBufferFromEntity<AcceptedDecisions>(true),
                internalDecisions = GetBufferFromEntity<DecisionInternal>(false),
                signalReferences = GetComponentDataFromEntity<SignalReferences>(false),
                cmds = cmds.ToConcurrent()
            }.Schedule(this, job);

            var job3 = new ClearBuffersJob {
                buffers = GetBufferFromEntity<AddNewTargets>(false),
            }.ScheduleSingle(this, job2);

            job3.Complete();

            cmds.Playback(EntityManager);
            cmds.Dispose();

            return default;
        }
    }
}