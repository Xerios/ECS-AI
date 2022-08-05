using Engine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

public class DeathSeat : MonoSingleton<DeathSeat>
{
    // private Entity entity;

    public int count;

    // Use this for initialization
    void Start ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        /*entity = mgr.CreateEntity(AIManager.SignalArchetype);
           mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
           mgr.SetComponentData(entity, new SignalActionType { data = UtilityAI.DecisionTags.Weapon });
           mgr.SetComponentData(entity, new SignalFlagsType { Flags = SignalFlags.NONE });
           mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 20f,
                limit = 10,
            });

           mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

           mgr.AddSharedComponentData(entity, new SignalAction {
                data = CollectMe
            });*/
    }

    // public Interaction InteractionSequence ()
    // {
    //     var interact = new InteractionMeta("Die");

    //     interact.SetUpdate((data) => {
    //             var targetPos = transform.position;
    //             targetPos.x += (count % 110) * 4f - transform.localScale.x * 0.5f;
    //             targetPos.z -= Mathf.FloorToInt((float)count / 110) * 10f;

    //             data.SetVec("destination", targetPos);
    //         })

    //     .Add("move", new AgentActions.MoveToState(arrivalDist: 1.75f), OnSuccess: "animate")

    //     .Add("animate", new AgentActions.AnimateState("Die"), Next: "position")
    //     .Add("position", new CustomAction(
    //         OnActivate: (data) => {
    //             var agent = data.Get<Agent>("self_agent");
    //             agent.transform.eulerAngles = new Vector3(0, 180, 0);
    //         },
    //         OnUpdate: (data, exit) => {
    //             return ActionResult.Running;
    //         })
    //         );


    //     return interact.Build();
    // }
}