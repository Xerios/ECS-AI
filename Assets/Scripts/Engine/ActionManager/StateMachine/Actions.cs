using Engine;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

public static class ActionsTest
{
    public static void Suspend (Context context) => context.Suspend();

    public static void Wait (Context context) => context.Wait(Time.time + 2);

    public static void ShortWait (Context context) => context.Wait(Time.time + 1);

    public static void StopAvoidance (Context context)
    {
        var data = context.data;
        var agent = data.Get<Agent>("self_agent");

        agent.NavMeshAgent.enabled = false;
    }

    public static void ResumeAvoidance (Context context)
    {
        var data = context.data;
        var agent = data.Get<Agent>("self_agent");

        agent.NavMeshAgent.enabled = true;
    }

    public static void StopIfNoLongerExists (Context context)
    {
        EntityManager mgr = AIManager.Instance.mgr;
        var target = context.GetTarget();
        var exists = mgr.Exists(target);

        if (!exists) context.Stop();
    }

    public static void Fail (Context context)
    {
        Mind.FailCurrentDecision(context.SelfMind, UnityEngine.Time.time, context.decisionHistory);
        // Forget(context);
    }
    public static void Forget (Context context)
    {
        if (!Bootstrap.world.EntityManager.HasComponent<DecisionId>(context.decisionHistory.DecisionEntity)) {
            Debug.LogWarning($"Can't forget somethign that doesn\'t exist??? {context.decisionHistory.DecisionEntity.Index}");
            return;
        }
        Bootstrap.world.GetExistingSystem<ActionManagerSystem>().PostUpdateCommands.AddComponent(context.decisionHistory.DecisionEntity, new DecisionForget());
    }
    public static void Repeat (Context context)
    {
        // Debug.Log("REPEAT: " + context.machine.Current.CurrentState.CurrentTrack.CurrentIndex);
        Mind.EndCurrentDecision(context.SelfMind, UnityEngine.Time.time, context.decisionHistory);
        Mind.StartCurrentDecision(context.SelfMind, UnityEngine.Time.time, context.decisionHistory, Entity.Null);
        context.Reset();
        // Debug.Log("REPEAT_2: " + context.machine.Current.CurrentState.CurrentTrack.CurrentIndex);
        // context.machine.Current.CurrentState.Begin();
    }

    public static void MoveToDestination (Context context)
    {
        var data = context.data;

        var agent = data.Get<Agent>("self_agent");
        var destination = data.GetVec("destination");

        if (agent.NavMeshAgent.destination != destination) {
            agent.NavMeshAgent.enabled = true;
            agent.NavMeshAgent.speed = 15;

            agent.NavMeshAgent.SetDestination(destination);
            // if (!valid) { DOESN'T WORK
            //     Debug.LogError("Path not valid! Reset");
            //     context.machine.Current.CurrentState.Begin();
            // }
        }
    }

    public static void StopMovement (Context context)
    {
        var data = context.data;
        var agent = data.Get<Agent>("self_agent");

        if (agent.NavMeshAgent.enabled) agent.NavMeshAgent.ResetPath();
    }


    public static void HasArrivedToDestination (Context context)
    {
        var data = context.data;
        var agent = data.Get<Agent>("self_agent");
        var destination = data.GetVec("destination");

        const float arrivalDistSqr = 2f;

        var hasReached = Vector2.SqrMagnitude(destination.XZ() - agent.GetPosition().XZ()) < arrivalDistSqr;

        if (!hasReached) context.Suspend();
    }
}