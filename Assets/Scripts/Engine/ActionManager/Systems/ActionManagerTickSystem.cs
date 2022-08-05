using Engine;
using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class ActionManagerTickSystem : ComponentSystem
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

            UnityEngine.Profiling.Profiler.BeginSample("Tick Nodes");

            Entities.With(m_Group).ForEach((Entity entity, ActionManagerSelf self) => {
                    self.Value.Tick(time);
                });

            UnityEngine.Profiling.Profiler.EndSample();


            // Necessary after all Ticks for proper sync, DO NOT PUT IT RIGHT AFTER EVERY(!!) TICK
            Entities.With(m_Group).ForEach((Entity entity, ActionManagerSelf self) => {
                    self.Value.Sync();
                });
        }

        internal void MasterTick ()
        {
            float time = Time.time;
        }
    }
}