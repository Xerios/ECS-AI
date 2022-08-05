using Engine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_Table : EntityMonoBehaviour
{
    private bool isTaken = false;
    private Entity isTakenBy;

    public AccessTags accessTags;

    public Transform sitPosition;

    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Trigger });
        mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 20f,
                // limit = 10,
            });


        mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

        mgr.AddComponentData(entity, new AccessTagData { Value = (uint)accessTags });

        mgr.AddSharedComponentData(entity, new ScoreEvaluate { Evaluate = (ent) => (isTakenBy.Equals(Entity.Null) || isTakenBy.Equals(ent)) ? 1f : 0f });
        // mgr.AddSharedComponentData(entity, new SignalAction {
        //         data = InteractionSequence
        //     });
    }

    // public Interaction InteractionSequence ()
    // {
    //     var interact = new InteractionMeta("Work");

    //     interact.Blackboard.SetVec("destination", sitPosition.position);


    //     interact
    //     .Add("is_taken", new ConditionalAction((data) => !isTaken || isTakenBy.Equals(data.Self)), OnSuccess: "take", OnFailure: "fail")
    //     .Add("take", new ActiveAction(
    //         OnActivate: (data) => {
    //             isTaken = true;
    //             isTakenBy = data.Self;

    //             data.SetBool("sit", true);
    //         },
    //         OnDeactivate: (data) => {
    //             isTaken = false;
    //             isTakenBy = Entity.Null;
    //         }),
    //         Next: "move"
    //         )
    //     .Add("move", new AgentActions.MoveToState(arrivalDist: 1.75f), OnSuccess: "position")

    //     .Add("position", new ActiveAction(
    //         OnActivate: (data) => {
    //             var agent = data.Get<Agent>("self_agent");
    //             agent.transform.localPosition = sitPosition.position;

    //             agent.OverrideRotation = true;
    //             agent.Rotation = sitPosition.rotation.eulerAngles.y;
    //         },
    //         OnDeactivate: (data) => {
    //             data.Get<Agent>("self_agent").OverrideRotation = false;
    //         })
    //         , Next: "animate")
    //     .Add("animate", new AgentActions.AnimateState("Sit", true), OnSuccess: "norepeat")
    //     .Add("fail", new AgentActions.FailDecisionState())
    //     .Add("norepeat", new AgentActions.DontRepeat());


    //     return interact.Build();
    // }


    private void OnDrawGizmos ()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = isTaken ? Color.red : Color.green;
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 5);

        if (!isTakenBy.Equals(Entity.Null)) {
            isTakenBy.Equals(Entity.Null);
            var pos = AIManager.Instance.mgr.GetComponentData<SignalPosition>(isTakenBy).Value;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position + Vector3.up, pos + Vector3.up);
        }
    }
}