using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [UpdateBefore(typeof(AIManager))]
    public class MindInitSystem : ComponentSystem
    {
        EntityQuery added, removed;

        public struct MindInitialized : ISystemStateComponentData {}

        protected override void OnCreateManager ()
        {
            added = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(MindBelongsTo)),
                    ComponentType.Exclude(typeof(MindInitialized))
                    );
        }


        protected override void OnUpdate ()
        {
            var cmds = PostUpdateCommands;

            Entities.With(added).ForEach((Entity entity, ref MindBelongsTo self) => {
                    EntityManager.GetSharedComponentData<AgentSelf>(self.Value).Value.GetMind().SetEntity(entity);
                    cmds.AddComponent(self.Value, new MindReference { Value = entity });
                    cmds.AddComponent(entity, new MindInitialized());
                });
        }
    }
}