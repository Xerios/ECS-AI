using Engine;
using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_HouseOld : EntityMonoBehaviour
{
    // public UniqueId id = UniqueId.Create("Building");

    private ParticipatantLayeredCollection participants = new ParticipatantLayeredCollection((uint)AccessTags.Human, (uint)AccessTags.Animal);

    public AccessTags accessTags;
    public GameObject enableOnFull;
    public GameObject[] spawnPrefabOnComplete;
    private bool prefabCreated;
    private int lastCreated;

    private float reEnableTime = -1;

    private const int POPULATION_INCREASE = 5;

    public void Start ()
    {
        // this.name = this.name + " " + id.ToString();
    }

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

        mgr.AddSharedComponentData(entity, new ScoreEvaluate {
                Evaluate = (ent) => {
                    // if (!AIManager.Instance.mgr.HasComponent<AccessTagData>(ent)) return 0f;
                    var myAccessTag = AIManager.Instance.mgr.GetComponentData<AccessTagData>(ent).Value;
                    return (participants.CanUse(ent, myAccessTag)) ? 1f : 0f;
                }
            });
        mgr.AddSharedComponentData(entity, new SignalAction {
                data = InteractionSequence
            });

        GameResources.Max.Population += POPULATION_INCREASE;
    }

    public override void DeInit ()
    {
        GameResources.Max.Population -= POPULATION_INCREASE;
    }

    private void Update ()
    {
        if (participants.IsFull) {
            if (!prefabCreated) {
                Instantiate(spawnPrefabOnComplete[lastCreated % spawnPrefabOnComplete.Length], this.transform.position, Quaternion.identity);
                AIManager.Instance.mgr.AddComponentData(entity, default(SignalBroadcastDisable));
                reEnableTime = Time.time + 2f;
                lastCreated++;
            }
            prefabCreated = true;
            enableOnFull.SetActive(true);
        }else{
            prefabCreated = false;
            enableOnFull.SetActive(false);
        }

        if (reEnableTime > 0 && Time.time > reEnableTime) {
            AIManager.Instance.mgr.RemoveComponent<SignalBroadcastDisable>(entity);
        }
    }


    public StateScript InteractionSequence ()
    {
        return null;
        // new StateScript("Go Inside House",
        //            new StateDefinition("get_to_house")// --------------------------------------
        //            .OnBegin(
        //            (context) => context.data.SetVec("destination", context.data.GetVec("target_position")),
        //            ActionsTest.MoveToDestination,
        //            ActionsTest.HasArrivedToDestination,
        //            (context) => context.Go("inside")
        //            ),
        //            new StateDefinition("inside")// --------------------------------------
        //            .OnBegin(
        //            (context) => {
        //                var myAccessTag = AIManager.Instance.mgr.GetComponentData<AccessTagData>(context.data.Self).Value;
        //                if (!participants.CanUse(context.data.Self, myAccessTag)) {
        //                    ActionsTest.Fail(context);
        //                    return;
        //                }
        //                participants.Add(context.data.Self, myAccessTag);
        //                context.data.Get<Agent>("self_agent").Hide();
        //                //    Debug.Log("Hello world, I'm inside the house");
        //            },
        //            (context) => {
        //                if (!participants.IsFull) ActionsTest.Suspend(context);
        //            },
        //            ActionsTest.Wait,
        //            (context) => {
        //                participants.Remove(context.data.Self);
        //                context.data.Get<Agent>("self_agent").Show();
        //                //    Debug.Log("I'm done now!");
        //            },
        //            ActionsTest.Fail,
        //            ActionsTest.Suspend
        //            )
        //            .OnExit((context) => {
        //                //    Debug.Log("Force exit");
        //                context.data.Get<Agent>("self_agent").Show();
        //                participants.Remove(context.data.Self);
        //            })
        //            .OnEnd(ActionsTest.Forget)
        //            );
    }

    private void OnDrawGizmos ()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = participants.IsEmpty ? Color.red : (participants.IsFull && Time.time % 1f > 0.5f ? Color.blue : Color.green);
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 5);
    }
}