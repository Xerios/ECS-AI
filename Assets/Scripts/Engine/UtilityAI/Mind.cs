using CircularBuffer;
using Engine;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    public class Mind : IDisposable
    {
        private Entity Entity;
        private Entity BelongsTo;

        public readonly List<Mindset> Mindsets = new List<Mindset>(); // Public only for editor

        public Entity GetEntity () => Entity;

        public void SetEntity (Entity entity)
        {
            Entity = entity;
            // Debug.Log("ADDED:" + entity);
            var mindsetsCopy = Mindsets.ToArray();
            Mindsets.Clear();
            foreach (var mindset in mindsetsCopy) AddMindset(mindset);
        }

        /// <summary>
        /// Initialize Mind
        /// </summary>
        public Mind(Entity self, EntityCommandBuffer cmds)
        {
            BelongsTo = self;

            Entity = cmds.CreateEntity(UtilityAIArchetypes.MindArchetype);
            cmds.SetComponent<MindBelongsTo>(Entity, new MindBelongsTo { Value = self });
            cmds.SetBuffer<DecisionInternal>(Entity);
            cmds.SetBuffer<AddNewTargets>(Entity);
#if UNITY_EDITOR
            // cmds.AddBuffer<DebugConsideration>(Entity);
#endif
        }

        public void Dispose ()
        {
            if (Entity == Entity.Null) {
                Debug.LogWarning("Mind already disposed");
                return;
            }
            // tag2decision.Clear();
            // decision2tag.Clear();
            AIManager.Instance.mgr.AddComponentData(Entity, new MindDestroyTag());

            Entity = Entity.Null;
        }

        // Setup
        // ---------------------------------------------------

        public void AddMindsets (params Mindset[] mindsets)
        {
            Mindsets.AddRange(mindsets);
            // SetEntity (Entity entity) handles the rest using AddMinset()
        }

        public void AddMindset (Mindset mindset)
        {
            if (Mindsets.Contains(mindset)) {
                Debug.LogWarning($"Already contains this mindset {mindset.Name} ( TO BE SOLVED )");
                return;
            }

            Mindsets.Add(mindset);

            Bootstrap.world.GetExistingSystem<MindUpdateSystem>().Add(BelongsTo, Entity, mindset);
        }

        public void RemoveMindset (Mindset mindset)
        {
            if (!Mindsets.Contains(mindset)) {
                Debug.LogWarning($"Doesn't have this mindset {mindset.Name} ( TO BE SOLVED )");
                return;
            }

            Mindsets.Remove(mindset);

            Bootstrap.world.GetExistingSystem<MindUpdateSystem>().Remove(BelongsTo, Entity, mindset);
        }

        public void RemoveAllMindset ()
        {
            for (int i = Mindsets.Count - 1; i >= 0; i--) {
                RemoveMindset(Mindsets[i]);
            }
        }

        // --------------------------------------------------------------------------

        public static void AddDecisionOption (Entity belongsTo, Entity entity, EntityCommandBuffer cmds, Decision dse)
        {
            UtilityAILoop.Instance.AddDecision(dse);

            // Debug.Log($"--{entity.Index} Add decisions {dse.Name} = {(DecisionTags)dse.Tags}");

            if (dse.Tags != 0) {
                // Debug.Log($"[{entity.Index}] Add Decision: {UtilityAILoop.Instance.calculateDecisionsSystem.decisions[dse.Id]}");

                var currentAcceptedDecisions = UtilityAILoop.Instance.mgr.GetBuffer<AcceptedDecisions>(entity);
                currentAcceptedDecisions.Add(new AcceptedDecisions(dse.Id));

                // Debug.Log($"--ADD-- {entity.Index}: {currentTags} + {dse.Tags}");

                return; // Create a decision if this doesn't require targets, AddDecisionTargetOption handles target related decisions
            }

            // Add Decision
            var newEntity = cmds.CreateEntity(UtilityAIArchetypes.DecisionArchetype);
            cmds.SetComponent(newEntity, new DecisionMindEntity { Value = entity });
            cmds.SetComponent(newEntity, new DecisionSelfEntity { Value = belongsTo });
            cmds.SetComponent(newEntity, new DecisionId { Id = dse.Id });
            cmds.SetComponent(newEntity, new DecisionPreferred { Value = 0f });
            cmds.SetComponent(newEntity, new DecisionTarget { Id = Entity.Null });

            AIManager.Instance.mgr.GetBuffer<DecisionInternal>(entity).Add(new DecisionInternal(dse.Id, newEntity, Entity.Null));
        }

        // --------------------------------------------------------------------------

        public static void RemoveDecisionOptions (Entity belongsTo, Entity entity, EntityCommandBuffer cmds, Decision dse)
        {
            // Debug.Log($"[{entity.Index}] RemoveDecisionOptions {UtilityAILoop.Instance.calculateDecisionsSystem.decisions[dse.Id]}");

            var buffer = AIManager.Instance.mgr.GetBuffer<DecisionInternal>(entity);

            for (int i = buffer.Length - 1; i >= 0; i--) {
                if (buffer[i].dseId == dse.Id) {
                    if (buffer[i].decisionEntity != Entity.Null) {
                        // Debug.Log($"[{entity.Index}] Remove Option {UtilityAILoop.Instance.calculateDecisionsSystem.decisions[buffer[i].dseId]}, {buffer[i].decisionEntity} => ({buffer[i].target})");

                        cmds.AddComponent(buffer[i].decisionEntity, new DecisionForget());
                        // buffer.RemoveAt(i); // IS THIS NECESSARY? FORGET WILL DELETE IT EITHER WAY NO????????????????????????????
                        // }else{
                        // Debug.Log($"Remove possible miss: {dse.Id} => {buffer[i].target}");
                    }
                }
            }

            // string debug = $"<b>RemoveDecisionOptions {belongsTo}:</b>: ({buffer.Length}): ";

            // for (int i = 0; i != buffer.Length; i++) debug += buffer[i].decisionEntity + ", ";
            // Debug.Log(debug);

            var currentAcceptedDecisions = UtilityAILoop.Instance.mgr.GetBuffer<AcceptedDecisions>(entity);
            currentAcceptedDecisions.Remove(new AcceptedDecisions(dse.Id));
        }

        // --------------------------------------------------------------------------

        public static void FailCurrentDecision (Entity entity, float Time, DecisionHistory hist)
        {
            if (hist.DecisionEntity == Entity.Null) return;

            // Debug.Log($"{entity.Index}: Fail {hist.DecisionEntity.Index} at time {Time}");

            var cmds = Bootstrap.world.GetExistingSystem<ActionManagerSystem>().PostUpdateCommands;

            if (Bootstrap.world.EntityManager.HasComponent<DecisionFailed>(hist.DecisionEntity)) {
                Debug.LogWarning($"{entity.Index}: Already fail {hist.DecisionEntity.Index}");
                return;
            }

            // cmds.SetComponent(hist.DecisionEntity, new DecisionScore { Value = 0f });
            cmds.SetComponent(hist.DecisionEntity, new DecisionPreferred { Value = 0f });
            cmds.AddComponent(hist.DecisionEntity, new DecisionFailed { Value = Time + 10f });
        }

        public static void StartCurrentDecision (Entity entity, float time, DecisionHistory hist, Entity newActiveDecision)
        {
            // Record history
            var dseId = hist.GetDSEId();
            var targetId = hist.GetSignalId();

            var buffer = AIManager.Instance.mgr.GetBuffer<DecisionHistoryRecord>(entity);

            if (buffer.Length != 0 && buffer[0].dseId == dseId && buffer[0].target == targetId) {
                if (buffer.Length == buffer.Capacity) {
                    buffer.RemoveAt(buffer.Capacity - 1);
                }
                buffer.Insert(0, new DecisionHistoryRecord { dseId = dseId, target = targetId, StartTime = time, EndTime = time });
            }

            // Set new active decision
            if (newActiveDecision != Entity.Null) {
                Bootstrap.world.GetExistingSystem<ActionManagerSystem>().PostUpdateCommands.SetComponent(entity, new ActiveDecision { entity = newActiveDecision, dseId = dseId, target = targetId });
            }
        }

        public static void EndCurrentDecision (Entity entity, float time, DecisionHistory hist)
        {
            // Is this necessary? There was a reason why I added this...

            // Record history
            var dseId = hist.GetDSEId();
            var targetId = hist.GetSignalId();

            var buffer = AIManager.Instance.mgr.GetBuffer<DecisionHistoryRecord>(entity);

            if (buffer.Length != 0) {
                if (buffer[0].dseId == dseId && buffer[0].target == targetId) {
                    var start = buffer[0].StartTime;
                    buffer.RemoveAt(0);
                    if (buffer.Length == 0) {
                        buffer.Add(new DecisionHistoryRecord { dseId = dseId, target = targetId, StartTime = start, EndTime = time });
                    }else{
                        buffer.Insert(0, new DecisionHistoryRecord { dseId = dseId, target = targetId, StartTime = start, EndTime = time });
                    }
                }
            }
        }
    }
}