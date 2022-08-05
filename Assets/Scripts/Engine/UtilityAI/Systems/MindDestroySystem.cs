using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class MindDestroySystem : ComponentSystem
    {
        UtilityAILifeCycleAfterDestroyBarrier barrier;
        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(ComponentType.ReadOnly(typeof(MindDestroyTag)), typeof(DecisionInternal));

            barrier = World.GetExistingSystem<UtilityAILifeCycleAfterDestroyBarrier>();
        }

        protected override void OnUpdate ()
        {
            // if (m_Group.CalculateLength() == 0) return;

            // var cmds = barrier.CreateCommandBuffer();

            // Debug.Log("Decision----");
            Entities.With(m_Group).ForEach((Entity entity, DynamicBuffer<DecisionInternal> decisions) => {
                    // string debug = $"Decision destroy ( all ): {entity} ({decisions.Length}): ";
                    for (int i = 0; i != decisions.Length; i++) {
                        // debug += decisions[i].decisionEntity + ", ";
                        // Debug.Log($"Decision destroy decision: {entity} > {decisions[i].decisionEntity}");
                        PostUpdateCommands.DestroyEntity(decisions[i].decisionEntity);
                    }
                    // Debug.Log(debug);
                    decisions.Clear();
                    PostUpdateCommands.DestroyEntity(entity);
                });

            // cmds.Playback(EntityManager);
        }
    }
}