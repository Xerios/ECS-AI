using Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

public class TiledBuildEffect : MonoBehaviour
{
    public GameObject[] Sequence;

    private Entity entity;
    private float progress;

    // Start is called before the first frame update
    void Start ()
    {
        GetComponent<EntityMonoBehaviour>().NoAutoInit();
        // StartCoroutine(TiledBuildCoroutine());
        foreach (var item in Sequence) item.SetActive(false);
        Sequence[0].SetActive(true);

        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity(UtilityAIArchetypes.SignalArchetype);
        mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.Build });
        mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 40f,
                // limit = 10,
            });
        mgr.AddSharedComponentData(entity, new SignalAction {
                data = InteractionSequence
            });
    }

    public StateScript InteractionSequence ()
    {
        return new StateScript("Go To Build",
                   new StateDefinition("get_to_build"){
                       {
                           StateDefinition.____BEGIN____,

                           (context) => {
                               var targetpos = context.data.GetVec("target_position");
                               var mypos = context.data.Get<Agent>("self_agent").GetPosition();
                               context.data.SetVec("destination", targetpos.Around(mypos, 5f));
                           },
                           ActionsTest.MoveToDestination,
                           ActionsTest.HasArrivedToDestination,
                           (context) => context.Go("build")
                       },
                       {
                           StateDefinition.____EXIT____,
                           ActionsTest.StopMovement
                       }
                   }
                   ,
                   new StateDefinition("build"){
                       {
                           StateDefinition.____BEGIN____,
                           ActionsTest.ShortWait,
                           (context) => {
                               //    Debug.Log("Build..");
                               var data = context.data;
                               var agent = data.Get<Agent>("self_agent");
                               agent.Animator.Play("Pickup", false);
                               Advance();

                               context.Repeat();
                           }
                       }
                   });
    }

    private void Advance ()
    {
        progress = Mathf.Clamp01(progress + 0.1f);

        if (progress == 1f) {
            StartCoroutine(FinalBuild());
        }else{
            foreach (var item in Sequence) item.SetActive(false);
            Sequence[Mathf.FloorToInt(progress * (Sequence.Length - 1))].SetActive(true);
        }
    }

    private IEnumerator FinalBuild ()
    {
        yield return null;
        var beh = GetComponent<EntityMonoBehaviour>();
        if (beh.IsInit()) yield break;
        beh.Init();
        foreach (var item in Sequence) item.SetActive(false);
        Sequence[Sequence.Length - 1].SetActive(true);
        AIManager.Instance.mgr.DestroyEntity(entity);
    }

    // private IEnumerator TiledBuildCoroutine ()
    // {
    //     foreach (var go in Sequence) {
    //         go.SetActive(false);
    //     }

    //     GameObject lastObject = null;
    //     foreach (var go in Sequence) {
    //         if (lastObject != null) lastObject.SetActive(false);

    //         go.SetActive(true);
    //         lastObject = go;
    //         yield return new WaitForSeconds(1f);
    //     }

    //     GetComponent<EntityMonoBehaviour>().Init();
    // }
}