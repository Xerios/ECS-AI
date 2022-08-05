using Engine;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    public class ActionManagerSystem : ComponentSystem
    {
        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(ActionManagerSelf)),
                    ComponentType.ReadOnly(typeof(MindReference)),
                    ComponentType.Exclude(typeof(InitAgent)));
        }

        protected override void OnUpdate ()
        {
            float time = Time.time;

            UnityEngine.Profiling.Profiler.BeginSample("Update Nodes");

            var bestDecision = GetComponentDataFromEntity<BestDecision>(true);

            Entities.With(m_Group).ForEach((Entity entity, ActionManagerSelf self, ref MindReference mind) => {
                    self.Value.Update(time, mind.Value, bestDecision[mind.Value].data);
                });

            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}