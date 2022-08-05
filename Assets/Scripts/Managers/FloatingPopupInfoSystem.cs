using Engine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UtilityAI;


public class FloatingPopupInfoSystem : ComponentSystem
{
    private struct FloatingPopupState : ISystemStateComponentData { }

    private EntityQuery m_AddedGroup, m_Group, m_RemovedGroup;
    private GameObject prefab;
    private Transform container;

    private Dictionary<Entity, FloatingPopupInfo> entity2gameObj;

    protected override void OnCreateManager ()
    {
        m_AddedGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(SignalAssigned)),
                ComponentType.Exclude(typeof(FloatingPopupState))
                );

        m_Group = GetEntityQuery(
                ComponentType.ReadOnly(typeof(SignalPosition)),
                ComponentType.ReadOnly(typeof(FloatingPopupState))
                );

        m_RemovedGroup = GetEntityQuery(
                ComponentType.Exclude(typeof(SignalAssigned)),
                ComponentType.ReadOnly(typeof(FloatingPopupState))
                );

        prefab = Resources.Load<GameObject>("UI/FloatingPopupInfo");
        container = GameObject.FindObjectOfType<CanvasScaler>()?.transform;

        entity2gameObj = new Dictionary<Entity, FloatingPopupInfo>();
    }

    protected override void OnUpdate ()
    {
        Entities.With(m_AddedGroup).ForEach((Entity entity, ref SignalAssigned assigned) => {
                // Debug.Log("Added new assignment");
                PostUpdateCommands.AddComponent(entity, new FloatingPopupState());

                var go = GameObject.Instantiate(prefab, container);
                var meta = go.GetComponent<FloatingPopupInfo>();
                meta.Set(entity, assigned.JobId);
                entity2gameObj.Add(entity, meta);
            });

        Entities.With(m_Group).ForEach((Entity entity, ref SignalPosition pos) => {
                entity2gameObj[entity].Position = pos.Value + Vector3.up;
            });

        Entities.With(m_RemovedGroup).ForEach((Entity entity, ref SignalPosition pos) => {
                // Debug.Log("Removed assignment");
                PostUpdateCommands.RemoveComponent<FloatingPopupState>(entity);

                if (entity2gameObj.TryGetValue(entity, out FloatingPopupInfo go)) {
                    entity2gameObj.Remove(entity);
                    GameObject.Destroy(go.gameObject);
                }
            });
    }
}