using Engine;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class SignalBroadcastSystem : JobComponentSystem
    {
        EntityQuery m_Group, m_TargetGroup, m_DestroyGroup;// , m_FactionGroup;

        private SpatialAwarnessSystem spatialSystem;

        private List<EntityWithFactionPosition> buffer;

        protected override void OnCreateManager ()
        {
            spatialSystem = World.GetExistingSystem<SpatialAwarnessSystem>();
            m_Group = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(SignalPosition)),
                    // ComponentType.ReadOnly(typeof(AgentFaction)),
                    ComponentType.ReadOnly(typeof(SignalBroadcast)),
                    ComponentType.ReadOnly(typeof(SignalActionType)),
                    ComponentType.ReadOnly(typeof(SignalFlagsType)),

                    ComponentType.Exclude(typeof(SignalBroadcastDisable))
                    );

            m_TargetGroup = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(SignalCastToTargets)),
                    ComponentType.ReadOnly(typeof(SignalActionType)),
                    ComponentType.ReadOnly(typeof(SignalFlagsType))
                    );

            m_DestroyGroup = GetEntityQuery(ComponentType.ReadOnly(typeof(SignalReferences)));

            // m_FactionGroup = GetEntityQuery(typeof(AgentFaction));
        }

        protected override void OnStartRunning ()
        {
            buffer = new List<EntityWithFactionPosition>(100);
        }

        struct InternalSignal
        {
            public Entity Receiver, Sender;
            public uint DecisionTags;
            public DecisionFlags DecisionFlags;

            public InternalSignal(Entity Item1, Entity Item2, uint Item3, DecisionFlags Item4)
            {
                this.Receiver = Item1;
                this.Sender = Item2;
                this.DecisionTags = Item3;
                this.DecisionFlags = Item4;
            }
        }

        [BurstCompile]
        [RequireComponentTag(typeof(SpatialTrackProximity), typeof(EntityWithFactionPosition))]
        private struct ProximityToTargetJob : IJobForEachWithEntity<SignalPosition, AgentFaction>
        {
            public BufferFromEntity<EntityWithFactionPosition> proximityBuffer;
            [WriteOnly] public BufferFromEntity<AddNewTargets> targetBuffer;
            [ReadOnly] public ComponentDataFromEntity<MindReference> references;

            public void Execute (Entity entity, int index, [ReadOnly] ref SignalPosition pos, [ReadOnly] ref AgentFaction faction)
            {
                DynamicBuffer<EntityWithFactionPosition> dynamicBuffer = proximityBuffer[entity];

                for (int i = 0; i < dynamicBuffer.Length; i++) {
                    var isMyFaction = ((dynamicBuffer[i].faction & faction.LayerFlags) == faction.LayerFlags);

                    if (!isMyFaction) targetBuffer[references[entity].Value].Add(new AddNewTargets(dynamicBuffer[i].entity, (uint)DecisionTags.Agent, DecisionFlags.NONE));
                }
                // dynamicBuffer.Clear();
            }
        }

        protected override JobHandle OnUpdate (JobHandle handle)
        {
            var time = Time.time;

            var references = GetComponentDataFromEntity<MindReference>(true);


            var jobDebug = new ProximityToTargetJob {
                proximityBuffer = GetBufferFromEntity<EntityWithFactionPosition>(),
                targetBuffer = GetBufferFromEntity<AddNewTargets>(),
                references = references
            }.ScheduleSingle(this, handle);

            jobDebug.Complete();
            // Entities.WithAll<SpatialTrackProximity>().ForEach((Entity entity, ref SignalPosition pos, DynamicBuffer<EntityInProximity> buffer) => {
            //         for (int i = 0; i < buffer.Length; i++) {
            //             EntityManager.GetBuffer<AddNewTargets>(references[entity].Value).Add(new AddNewTargets(buffer[i].entity, (uint)DecisionTags.Agent, DecisionFlags.NONE));
            //             // Debug.DrawLine(pos.Value + Vector3.up, buffer[i].position, Color.green, 0.4f);
            //         }
            //         buffer.Clear();
            //     });

            var entities = m_Group.ToEntityArray(Allocator.TempJob);
            var signalBroadcasters = m_Group.ToComponentDataArray<SignalBroadcast>(Allocator.TempJob);
            var positions = m_Group.ToComponentDataArray<SignalPosition>(Allocator.TempJob);
            var actionType = m_Group.ToComponentDataArray<SignalActionType>(Allocator.TempJob);
            var flags = m_Group.ToComponentDataArray<SignalFlagsType>(Allocator.TempJob);


            var addNewTargetBuffers = GetBufferFromEntity<AddNewTargets>(false);
            // var factions = m_Group.ToComponentDataArray<AgentFaction>(Allocator.TempJob);

            // var nativeHashmap = new NativeQueue<InternalSignal>(Allocator.TempJob);

            // Profiler.BeginSample("First");
            Profiler.BeginSample("Loop through all broadcasts");
            for (int i = 0; i != entities.Length; i++) {
                buffer.Clear();

                Profiler.BeginSample("GetDynamicsInRange");
                spatialSystem.GetDynamicsInRange(positions[i].Value.XZ(), signalBroadcasters[i].radius, buffer);// , signalBroadcasters[i].limit);
                Profiler.EndSample();

                var owner = entities[i];
                var count = buffer.Count;

                // byte layerFlags = byte.MaxValue;
                // if (factions.Exists(buffer[j].sender)) layerFlags = factions[buffer[j].sender].LayerFlags;

                for (int j = 0; j < count; j++) {
                    if (buffer[j].entity.Equals(owner)) continue;


                    if (!EntityManager.Exists(buffer[j].entity)) {
                        Debug.LogWarning("SignalBroadcast: Entity does not exist");
                        continue;
                    }
                    if (!references.Exists(buffer[j].entity)) {
                        Debug.LogWarning("SignalBroadcast: Entity does not have mindreference");
                        continue;
                    }

                    addNewTargetBuffers[references[buffer[j].entity].Value].Add(new AddNewTargets(entities[i], actionType[i].decisionTags, flags[i].Flags));
                    // nativeHashmap.Enqueue(new InternalSignal(buffer[j].entity, entities[i], actionType[i].decisionTags, flags[i].Flags));
                }
            }
            // Profiler.EndSample();

            entities.Dispose();
            signalBroadcasters.Dispose();
            positions.Dispose();
            actionType.Dispose();
            flags.Dispose();

            // Profiler.BeginSample("Iterate");

            // while (nativeHashmap.Count != 0) {
            //     var msg = nativeHashmap.Dequeue();
            //     byte faction = byte.MaxValue;
            //     if (factions.Exists(msg.Sender)) {
            //         faction = factions[msg.Sender].LayerFlags;
            //     }
            //     Agent agent = EntityManager.GetSharedComponentData<AgentSelf>(msg.Receiver).Value;
            //     if (agent != null) {
            //         agent.RespondToSignal(PostUpdateCommands, time, msg.Sender, msg.DecisionTags, msg.DecisionFlags, faction);
            //     }
            // }

            // Profiler.EndSample();

            // nativeHashmap.Dispose();
            Profiler.EndSample();

            // --------------------------
            entities = m_TargetGroup.ToEntityArray(Allocator.TempJob);
            actionType = m_TargetGroup.ToComponentDataArray<SignalActionType>(Allocator.TempJob);
            flags = m_TargetGroup.ToComponentDataArray<SignalFlagsType>(Allocator.TempJob);
            var signalTarget = m_TargetGroup.ToComponentDataArray<SignalCastToTargets>(Allocator.TempJob);
            // var factionsFromEntity = GetComponentDataFromEntity<AgentFaction>(true);


            void AddToTarget (ComponentDataFromEntity<MindReference> refs, Entity target, AddNewTargets newTarget)
            {
                if (!references.Exists(target)) {
                    Debug.LogWarning("SignalTarget: MindReference not intialized on this entity");
                    return;
                }

                var trget = references[target].Value;

                if (!EntityManager.HasComponent<AddNewTargets>(trget)) {
                    Debug.LogWarning("SignalTarget: AddNewTargets not intialized on this entity");
                    return;
                }
                // Debug.Log($"--{target.Index} AddTargetsJob {(DecisionTags)newTarget.decisionTags}");

                addNewTargetBuffers[trget].Add(newTarget);
            }

            var cmds = AIManager.Instance.buffer;


            for (int i = 0; i != entities.Length; i++) {
                // byte faction = byte.MaxValue;
                // if (factions.Exists(entities[i])) {
                //     faction = factions[entities[i]].LayerFlags;
                // }

                AddToTarget(references, signalTarget[i].Target, new AddNewTargets(entities[i], actionType[i].decisionTags, flags[i].Flags));

                if (!Entity.Null.Equals(signalTarget[i].Target2)) {
                    AddToTarget(references, signalTarget[i].Target2, new AddNewTargets(entities[i], actionType[i].decisionTags, flags[i].Flags));
                }
                if (!Entity.Null.Equals(signalTarget[i].Target3)) {
                    AddToTarget(references, signalTarget[i].Target3, new AddNewTargets(entities[i], actionType[i].decisionTags, flags[i].Flags));
                }
                if (!Entity.Null.Equals(signalTarget[i].Target4)) {
                    AddToTarget(references, signalTarget[i].Target4, new AddNewTargets(entities[i], actionType[i].decisionTags, flags[i].Flags));
                }
                cmds.RemoveComponent<SignalCastToTargets>(entities[i]);
            }

            entities.Dispose();
            actionType.Dispose();
            flags.Dispose();
            signalTarget.Dispose();

            // --------------------------
            entities = m_DestroyGroup.ToEntityArray(Allocator.TempJob);
            var shouldDestroy = m_DestroyGroup.ToComponentDataArray<SignalReferences>(Allocator.TempJob);

            for (int i = 0; i != entities.Length; i++) {
                if (shouldDestroy[i].Count == -1) cmds.SetComponent(entities[i], new SignalReferences { Count = 0 });
                else if (shouldDestroy[i].Count == 0) cmds.DestroyEntity(entities[i]);
            }
            entities.Dispose();
            shouldDestroy.Dispose();

            return default;
        }
    }
}