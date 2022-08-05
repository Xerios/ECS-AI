using Engine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class AgentInitSystem : ComponentSystem
    {
        EntityQuery m_Group;// , m_group2;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(ComponentType.ReadOnly(typeof(InitAgent)), ComponentType.ReadOnly(typeof(AgentSelf)));
        }

        protected override void OnUpdate ()
        {
            var cmds = PostUpdateCommands;

            Entities.With(m_Group).ForEach((Entity entity, AgentSelf self) => {
                    self.Value.InitSuccess(entity, cmds);
                    cmds.RemoveComponent<InitAgent>(entity);
                    cmds.AddComponent(entity, new SpatialTracking());
                    cmds.AddComponent(entity, new SpatialTrackProximity(30f));
                    cmds.AddComponent(entity, new SpatialDetectionTimer { timer = 0 });
                    cmds.AddComponent(entity, new SpatialDetectionState { state = SpatialDetectionState.NORMAL });
                    cmds.AddComponent(entity, new SpatialDetectionSpeed { speed = 2f });

                    cmds.AddBuffer<EntityWithFactionPosition>(entity);
                });
        }
    }
}