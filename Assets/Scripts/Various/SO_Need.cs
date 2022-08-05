using Engine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

public class SO_Need : EntityMonoBehaviour
{
    private bool isTaken = false;
    private Entity isTakenBy;

    public AccessTags accessTags;
    public NeedTags needsSatisfy;

    public Transform sitPosition;

    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        // needsSatisfy = new NeedTags {
        //     Water = -UnityEngine.Random.Range(10, 20),
        //     Food = -UnityEngine.Random.Range(10, 10),
        // };


        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

        mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Resource });
        mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 100f,
                // limit = 10,
            });

        mgr.AddComponentData(entity, new AccessTagData { Value = (uint)accessTags });
        mgr.AddComponentData(entity, new NeedsData { Value = needsSatisfy });

        // mgr.AddSharedComponentData(entity, new SignalAction {
        //         data = InteractionSequence
        //     });
    }

    // public Interaction InteractionSequence ()
    // {
    //     var interact = new InteractionMeta($"Satisfy {needsSatisfy}");

    //     interact
    //     .SetUpdate((data) => {
    //             data.SetVec("destination", sitPosition.position);
    //         })
    //     .Add("is_taken", new CustomAction(
    //         OnUpdate: (data, exit) => !isTaken || isTakenBy.Equals(data.Self) ? ActionResult.Success : ActionResult.Failure
    //         )
    //         , OnSuccess: "take",
    //         OnFailure: "fail")
    //     .Add("take", new CustomAction(
    //         OnActivate: (data) => {
    //             isTaken = true;
    //             isTakenBy = data.Self;
    //         },
    //         OnUpdate: (data, exit) => {
    //             if (exit) {
    //                 isTaken = false;
    //                 isTakenBy = Entity.Null;
    //                 return ActionResult.Exit;
    //             }
    //             return ActionResult.Success;
    //         },
    //         OnDeactivate: (data) => {
    //             isTaken = false;
    //             isTakenBy = Entity.Null;
    //         })
    //         , Next: "move"
    //         )

    //     .Add("move", new AgentActions.MoveToState(arrivalDist: 1.75f), OnSuccess: "position")

    //     .Add("position", new CustomAction(
    //         OnActivate: (data) => {
    //             var agent = data.Get<Agent>("self_agent");
    //             agent.transform.localPosition = sitPosition.position;

    //             agent.OverrideRotation = true;
    //             agent.Rotation = sitPosition.rotation.eulerAngles.y;

    //             var selfNeeds = AIManager.Instance.mgr.GetComponentData<NeedsData>(data.Self).Value;

    //             var targetNeedsSatisfy = AIManager.Instance.mgr.GetComponentData<NeedsData>(context.getTarget())).Value;
    //             AIManager.Instance.buffer.SetComponent<NeedsData>(data.Self, new NeedsData { Value = (selfNeeds + targetNeedsSatisfy).TruncateNegative() });
    //         },
    //         OnUpdate: (data, exit) => {
    //             if (exit) return ActionResult.Exit;
    //             return ActionResult.Running | ActionResult.Success;
    //         },
    //         OnDeactivate: (data) => {
    //             data.Get<Agent>("self_agent").OverrideRotation = false;
    //         })
    //         , Next: "animate")
    //     .Add("animate", new AgentActions.AnimateState("Pickup"), OnSuccess: "norepeat")
    //     .Add("fail", new AgentActions.FailDecisionState())
    //     .Add("norepeat", new AgentActions.DontRepeatThisTick());


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