using Engine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_Reception : EntityMonoBehaviour
{
    private Entity entityGuest;

    private BlackboardDictionary sharedBlackBoard;

    private bool isTaken = false;
    private Entity isTakenBy;
    private bool receptionistTalkedTo;
    private bool isGuestTaken;
    private bool isGuestReady;
    private Entity isGuestTakenBy;

    public AccessTags accessTag, accessTagGuest;

    public Transform sitPosition, sitPositionGuest;
    private bool guestWelcomed;

    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        // ---------------------------
        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Trigger });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 20f,
                // limit = 10,
            });
        mgr.SetSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });

        mgr.AddComponentData(entity, new AccessTagData { Value = (uint)accessTag });

        mgr.AddSharedComponentData(entity, new ScoreEvaluate { Evaluate = (ent) => (isTakenBy.Equals(Entity.Null) || isTakenBy.Equals(ent)) ? 1f : 0f });
        // mgr.AddSharedComponentData(entity, new SignalAction {
        //         data = ReceptionistSequence
        //     });
        // ---------------------------
        entityGuest = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entityGuest, new SignalPosition { Value = transform.position });
        mgr.SetComponentData(entityGuest, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Trigger });
        mgr.SetComponentData(entityGuest, new SignalBroadcast {
                radius = 20f,
                // limit = 10,
            });
        mgr.SetSharedComponentData(entityGuest, new SignalGameObject { Value = this.gameObject });

        mgr.AddComponentData(entityGuest, new AccessTagData { Value = (uint)accessTagGuest });

        mgr.AddSharedComponentData(entityGuest, new ScoreEvaluate { Evaluate = (ent) => (isGuestTakenBy.Equals(Entity.Null) || isGuestTakenBy.Equals(ent)) ? 1f : 0f });
        // mgr.AddSharedComponentData(entityGuest, new SignalAction {
        //         data = GuestSequence
        //     });

        // ---------------

        sharedBlackBoard = new BlackboardDictionary();

        // sharedBlackBoard.SetBool("guest_available", false);
        // // sharedBlackBoard.SetEntity("guest_entity", Entity.Null);
        // sharedBlackBoard.SetVec("guest_pos", sitPositionGuest.position);
        // sharedBlackBoard.SetBool("guest_talked", false);

        // sharedBlackBoard.SetBool("reception_available", false);
        // // sharedBlackBoard.SetEntity("reception_entity", Entity.Null);
        // sharedBlackBoard.SetVec("reception_pos", sitPosition.position);
        // sharedBlackBoard.SetBool("reception_talked", false);
    }

    // public Interaction ReceptionistSequence ()
    // {
    //     var interact = new InteractionMeta("Work As Receptionist", new BlackboardDictionary(sharedBlackBoard));

    //     interact.Blackboard.SetVec("destination", sitPosition.position);

    //     interact
    //     .Add("is_taken", new ConditionalAction((data) => !isTaken || isTakenBy.Equals(data.Self)), OnSuccess: "take", OnFailure: "fail")
    //     .Add("take", new ActiveAction(
    //         OnActivate: (data) => {
    //             isTaken = true;
    //             isTakenBy = data.Self;

    //             receptionistTalkedTo = false;

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
    //         , Next: "should_be_sitting")
    //     .Add("should_be_sitting", new AgentActions.IsBoolActive("sit"), OnSuccess: "animate")
    //     .Add("animate", new AgentActions.AnimateState("Sit", true), Next: "guest_ready")
    //     .Add("guest_ready", new ConditionalAction((data) => isGuestTaken && isGuestReady), OnSuccess: "do_actions")
    //     .Add("do_actions", new CustomAction(
    //         OnUpdate: (data, exit) => {
    //             if (exit) return ActionResult.Exit;

    //             var agent = data.Get<Agent>("self_agent");

    //             var isSitTalkAnimating = agent.Animator.IsPlayingAnimation("SitTalk");

    //             if (!guestWelcomed && !isSitTalkAnimating) {
    //                 guestWelcomed = true;
    //                 agent.Animator.PlayAnimation("SitTalk");
    //                 isSitTalkAnimating = true;
    //             }

    //             if (!isSitTalkAnimating) {
    //                 // data.SetBool("sit", false);
    //                 return ActionResult.Success;
    //             }

    //             return ActionResult.Running;
    //         }), OnFinished: "work_now")
    //     .Add("work_now", new RunOnceAction(
    //         (data) => {
    //             var agent = AIManager.Instance.mgr.GetSharedComponentData<AgentSelf>(isGuestTakenBy).Value;
    //             agent.AccessTag = AccessTags.Worker;
    //             AIManager.Instance.buffer.SetComponent(isGuestTakenBy, new AccessTagData { Value = (uint)agent.AccessTag });
    //         }))
    //     .Add("fail", new AgentActions.FailDecisionState());


    //     return interact.Build();
    // }

    // public Interaction GuestSequence ()
    // {
    //     var interact = new InteractionMeta("Sit As Guest");// , blackboard);

    //     interact.Blackboard.SetVec("destination", sitPositionGuest.position);

    //     interact
    //     .Add("is_taken", new ConditionalAction((data) => !isGuestTaken || isGuestTakenBy.Equals(data.Self)), OnSuccess: "take", OnFailure: "fail")
    //     .Add("take", new ActiveAction(
    //         OnActivate: (data) => {
    //             isGuestTaken = true;
    //             isGuestTakenBy = data.Self;

    //             isGuestReady = false;
    //             guestWelcomed = false;

    //             receptionistTalkedTo = false;

    //             data.SetBool("sit", true);
    //         },
    //         OnDeactivate: (data) => {
    //             isGuestTaken = false;
    //             isGuestTakenBy = Entity.Null;

    //             isGuestReady = false;
    //             guestWelcomed = false;
    //         }),
    //         Next: "move"
    //         )

    //     .Add("move", new AgentActions.MoveToState(arrivalDist: 1.75f), OnSuccess: "position")

    //     .Add("position", new ActiveAction(
    //         OnActivate: (data) => {
    //             var agent = data.Get<Agent>("self_agent");
    //             agent.transform.localPosition = sitPositionGuest.position;

    //             agent.OverrideRotation = true;
    //             agent.Rotation = sitPositionGuest.rotation.eulerAngles.y;
    //         },
    //         OnDeactivate: (data) => {
    //             data.Get<Agent>("self_agent").OverrideRotation = false;
    //         })
    //         , Next: "should_be_sitting")
    //     .Add("should_be_sitting", new AgentActions.IsBoolActive("sit"), OnSuccess: "animate")
    //     .Add("animate", new AgentActions.AnimateState("Sit", true), Next: "do_actions")
    //     .Add("do_actions", new CustomAction(
    //         OnUpdate: (data, exit) => {
    //             if (exit) return ActionResult.Exit;
    //             if (!isTaken) return ActionResult.Running;

    //             var agent = data.Get<Agent>("self_agent");
    //             var isSitTalkAnimating = agent.Animator.IsPlayingAnimation("SitTalk");

    //             if (!receptionistTalkedTo && !isSitTalkAnimating) {
    //                 isGuestReady = true;
    //                 receptionistTalkedTo = true;
    //                 agent.Animator.PlayAnimation("SitTalk");
    //                 isSitTalkAnimating = true;
    //             }

    //             // if (!isSitTalkAnimating) {
    //             //     data.SetBool("sit", false);
    //             //     return ActionResult.Success;
    //             // }

    //             return ActionResult.Running;
    //         }))
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

        if (!isGuestTakenBy.Equals(Entity.Null)) {
            isGuestTakenBy.Equals(Entity.Null);
            var pos = AIManager.Instance.mgr.GetComponentData<SignalPosition>(isGuestTakenBy).Value;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(this.transform.position + Vector3.up, pos + Vector3.up);
        }
    }
}