using Engine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class CoinCollectActionTest : MonoBehaviour
{
    private Entity entity;

    // Use this for initialization
    void Start ()
    {
        var CurrentPosition = this.transform.position;

        // AIManager.Instance.mgr..AddToEntityManager(mgr, gameObject)

        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity(mgr.CreateArchetype(typeof(SignalPosition), typeof(SignalActionType), typeof(SignalFlagsType), typeof(SignalBroadcast)));
        mgr.SetComponentData(entity, new SignalPosition { Value = CurrentPosition });
        mgr.SetComponentData(entity, new SignalActionType { decisionTags = (uint)UtilityAI.DecisionTags.TriggerAction });
        mgr.SetComponentData(entity, new SignalFlagsType { Flags = DecisionFlags.NONE });
        mgr.AddComponentData(entity, new SignalBroadcast {
                radius = 20f,
                // limit = 100,
            });
        /*mgr.AddSharedComponentData(entity, new SignalAction {
                data = CollectMe
            });*/

        // Debug.Log($"#{entity.Index} {mgr.GetComponentData<SignalActionType>(entity).data}");

        this.name = $"Coin #{entity.Index}";
    }

    void Update ()
    {
        AIManager.Instance.mgr.SetComponentData(entity, new SignalPosition { Value = transform.position });
        // Bootstrap.world.GetOrCreateManager<SignalBroadcastSystem>().PostUpdateCommands.SetComponent(
    }

    private bool isQuiting = false;
    void OnApplicationQuit ()
    {
        isQuiting = true;
    }

    void OnDestroy ()
    {
        if (isQuiting) return;
        AIManager.Instance.mgr.DestroyEntity(entity);
    }

    /*SequenceAction CollectMe ()
       {
        return SequenceAction
               .New("Move To", (self) => {
                       var pos = AIManager.Instance.mgr.GetComponentData<SignalPosition>(self.GetSequencePlayer().CurrentContext.Target);
                       return (self as Agent).GoTo(pos.Value);
                   })
               .Then("Arrived?", (self) => {
                       return (self as Agent).IsArrived(1.75f);
                   })
               .Then("Remove", (self) => {
                       GameObject.Destroy(this.gameObject);
                       return ActionResult.SuccessImmediate;
                   });
       }*/

    public float EvaluateScore (Entity target)
    {
        /*if (!agent.name.StartsWith("Jew")) {
            return 0f;
           }*/

        var from = AIManager.Instance.mgr.GetComponentData<SignalPosition>(this.entity).Value;
        var to = AIManager.Instance.mgr.GetComponentData<SignalPosition>(target).Value;
        var dist = Vector3.SqrMagnitude(from - to);
        const float min = 1, max = 10;

        float minSqr = (min * min);
        float maxSqr = (max * max);

        float value = (dist - minSqr) / (maxSqr - minSqr);

        return 1 - (value * value * value);
    }
}