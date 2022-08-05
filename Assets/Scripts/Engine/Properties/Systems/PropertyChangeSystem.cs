using Engine;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

namespace Game
{
    [DisableAutoCreation]
    public class PropertyChangeSystem : ComponentSystem
    {
        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(typeof(PropertyChangeMessage));
        }

        protected override void OnUpdate ()
        {
            int Length = m_Group.CalculateLength();
            var entities = m_Group.ToEntityArray(Allocator.TempJob);
            var message = m_Group.ToComponentDataArray<PropertyChangeMessage>(Allocator.TempJob);

            for (int i = 0; i != Length; i++) {
                var buffer = EntityManager.GetBuffer<PropertyData>(message[i].Target);

                for (int j = 0; j < buffer.Length; j++) {
                    if (message[i].Type == buffer[j].Type) {
                        buffer[j] = new PropertyData(buffer[j].Type, buffer[j].Value + message[i].Value);
                        break;
                    }
                }

                PostUpdateCommands.DestroyEntity(entities[i]);
            }

            entities.Dispose();
            message.Dispose();
        }
    }
}