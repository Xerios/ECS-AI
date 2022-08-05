using Engine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_Resource : EntityMonoBehaviour
{
    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.AddComponentData(entity, new SignalAbilityAssignment { Ability = (byte)UtilityAI.AbilityTags.Gather });
        // mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Resource });
        // mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });

        // mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        // mgr.AddComponentData(entity, new SignalBroadcast {
        //         radius = 60f,
        //         limit = 30,
        //     });

        // mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

        // mgr.AddComponentData(entity, new AccessTagData { Value = (uint)accessTags });

        // mgr.AddSharedComponentData(entity, new ScoreEvaluate {
        //         Evaluate = (ent) => 1f
        //     });
        // mgr.AddSharedComponentData(entity, new SignalAction {
        //         data = InteractionSequence
        //     });
    }


    private void OnDrawGizmos ()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 1);
    }
}