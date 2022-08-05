using Engine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_Altar : EntityMonoBehaviour
{
    private ParticipatantCollection participants = new ParticipatantCollection(10);

    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Altar });
        mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 60f,
                // limit = 20,
            });

        mgr.AddSharedComponentData(entity, new ScoreEvaluate { Evaluate = (ent) => participants.CanUse(ent) ? 1f : 0f });

        mgr.AddSharedComponentData(entity, new SignalAction { data = InteractionSequence });
    }

    public StateScript InteractionSequence ()
    {
        return new StateScript("Go To Altar",
                   new StateDefinition("inside"){
                       {
                           StateDefinition.____BEGIN____,
                           (context) => {
                               if (!participants.CanUse(context.data.Self)) {
                                   ActionsTest.Fail(context);
                                   return;
                               }
                               var mypos = context.data.Get<Agent>("self_agent").GetPosition();

                               var diff = mypos - this.transform.position;
                               var corrected = -diff.normalized.DirectionXZ() + 180f;
                               var preferedPlace = Mathf.FloorToInt(corrected / 360f * participants.Capacity);

                               participants.Add(context.data.Self, preferedPlace);
                           },
                           (context) => {
                               if (!participants.IsFull) ActionsTest.Suspend(context);
                               context.Go("get_to_altar");
                           }
                       }
                   },
                   new StateDefinition("get_to_altar"){
                       {
                           StateDefinition.____BEGIN____,
                           (context) => {
                               var myId = participants.GetIndex(context.data.Self);
                               //    var floor = Mathf.Floor(myId / 10f);
                               var getIdNormalized = (float)participants.GetIndex(context.data.Self) / participants.Capacity;
                               var getIdRadius = getIdNormalized * Mathf.PI * 2 - Mathf.PI / 2f;
                               var circleVec = new Vector3(Mathf.Cos(getIdRadius), 0, Mathf.Sin(getIdRadius)) * 5f;

                               var targetpos = context.data.GetVec("target_position");
                               var mypos = context.data.Get<Agent>("self_agent").GetPosition();
                               context.data.SetVec("destination", targetpos + circleVec);
                           },
                           ActionsTest.MoveToDestination,
                           ActionsTest.HasArrivedToDestination,
                           ActionsTest.Wait,
                           ActionsTest.StopAvoidance,
                           (context) => context.Go("pray")
                       },
                       {
                           StateDefinition.____EXIT____,
                           ActionsTest.ResumeAvoidance,
                           (context) => participants.Remove(context.data.Self)
                       }
                   },
                   new StateDefinition("pray"){
                       {
                           StateDefinition.____BEGIN____,
                           ActionsTest.ShortWait,
                           (context) => {
                               //    Debug.Log("Faith gained");
                               var data = context.data;
                               var agent = data.Get<Agent>("self_agent");
                               agent.Animator.Play("Pickup", false);
                               GameResources.Current.Faith++;


                               StaminaData.Add(context.data.Self, -1f);
                               // if (this.gameObject != null) Destroy(this.gameObject, 0.1f);
                               context.Repeat();
                           }
                       },
                       {
                           StateDefinition.____EXIT____,
                           ActionsTest.ResumeAvoidance,
                           (context) => participants.Remove(context.data.Self)
                       }
                   });
    }

    private void OnDrawGizmos ()
    {
        if (!Application.isPlaying) return;

        // RaycastManager.Instance.RaycastGround(Input.mousePosition, out Vector3 hitpos);
        // var diff = hitpos - this.transform.position;
        // Gizmos.DrawLine(this.transform.position, this.transform.position + diff);
        // var corrected = -diff.normalized.DirectionXZ() + 180f;
        // Debug.Log(Mathf.FloorToInt(corrected / 360f * participants.Capacity));


        Gizmos.color = Color.green;

        foreach (var ent in participants.GetNonEmptyEntities()) {
            var pos = AIManager.Instance.mgr.GetComponentData<SignalPosition>(ent).Value;
            Gizmos.DrawLine(this.transform.position + Vector3.up, pos + Vector3.up);
        }

        Gizmos.color = participants.IsEmpty ? Color.red : (participants.IsFull && Time.time % 1f > 0.5f ? Color.blue : Color.green);
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 1);
    }
}