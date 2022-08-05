using Engine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class WeaponTest : EntityMonoBehaviour
{
    public Mindset MindsetToAddOnWeaponPickup;

    public override void Init ()
    {
        var CurrentPosition = this.transform.position;

        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entity, new SignalPosition { Value = CurrentPosition });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Weapon });
        mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 20f,
                // limit = 10,
            });

        mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

        this.name = $"WeaponTest #{entity.Index}";
    }

    void Update ()
    {
        if (entity.Equals(Entity.Null)) return;

        AIManager.Instance.mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
    }
}