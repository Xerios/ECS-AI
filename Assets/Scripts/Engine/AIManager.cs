using CircularBuffer;
using Engine;
using Game;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;
using UtilityAI;

// [DisableAutoCreation]
[AlwaysUpdateSystem]
public class AIManager : ComponentSystem
{
    private static AIManager instance;
    public static AIManager Instance {
        get {
            if (instance != null) return instance;
            instance = World.Active.GetOrCreateSystem<AIManager>();
            return instance;
        }
    }

    public EntityManager mgr;

    public EntityCommandBuffer buffer;
    public Queue<Action> DeferredQueue = new Queue<Action>();

    private UtilityAILoop utilityAILoop;

    private SpatialAwarnessSystem spatialAwarnessSystem;
    private PropertyChangeSystem propertyChangeSystem;

    private AgentInitSystem agentInitSystem;
    private DetectionSystem detectionSystem;
    private ActionManagerTickSystem actionManagerTickSystem;
    private SignalBroadcastSystem signalBroadcastSystem;

    private InfluenceMapSystem influenceMapSystem;
    private DecisionTargetAddSystem targetAddDecisionSystem;
    // private FloatingPopupInfoSystem floatingPopupInfoSystem;
    private int doublebuffer = -1;

    protected override void OnCreateManager ()
    {
        World world = World.Active;

        mgr = world.EntityManager;

        utilityAILoop = new UtilityAILoop();
        propertyChangeSystem = world.GetOrCreateSystem<PropertyChangeSystem>();

        spatialAwarnessSystem = world.GetOrCreateSystem<SpatialAwarnessSystem>();
        signalBroadcastSystem = world.GetOrCreateSystem<SignalBroadcastSystem>();

        actionManagerTickSystem = world.GetOrCreateSystem<ActionManagerTickSystem>();

        agentInitSystem = world.GetOrCreateSystem<AgentInitSystem>();
        detectionSystem = world.GetOrCreateSystem<DetectionSystem>();

        influenceMapSystem = world.GetOrCreateSystem<InfluenceMapSystem>();

        targetAddDecisionSystem = world.GetOrCreateSystem<DecisionTargetAddSystem>();
    }


    public const float TICK_RATE = 0.083334f;
    private float nextUpdateTime;

    protected override void OnUpdate ()
    {
        var time = Time.time;

        if (time > nextUpdateTime) {
            nextUpdateTime = time + TICK_RATE;
            MasterTick();
        }
    }

    public void MasterTick ()
    {
        doublebuffer = (doublebuffer + 1) % 3;

        UnityEngine.Profiling.Profiler.BeginSample("Tick:" + doublebuffer);

        if (doublebuffer == 0) MasterTickOne();
        if (doublebuffer == 1) MasterTickTwo();
        if (doublebuffer == 2) MasterTickThree();

        UnityEngine.Profiling.Profiler.EndSample();
    }

    public void MasterTickOne ()
    {
        // Debug.Log("Tick");

        buffer = new EntityCommandBuffer(Allocator.Temp);
        while (DeferredQueue.Count != 0) DeferredQueue.Dequeue().Invoke();
        buffer.Playback(mgr);
        buffer.Dispose();

        // -----------------
        propertyChangeSystem.Update();

        // -----------------
        spatialAwarnessSystem.Update();
        spatialAwarnessSystem.Tick();     // Has to be here since Update only happens on Create/Destroy components

        // -----------------
        detectionSystem.Update();
        // -----------------

        // -----------------
        agentInitSystem.Update();

        // -----------------
        utilityAILoop.Tick();
    }

    public void MasterTickTwo ()
    {
        buffer = new EntityCommandBuffer(Allocator.Temp);
        // -----------------
        signalBroadcastSystem.Update();
        // -----------------
        buffer.Playback(mgr);
        buffer.Dispose();
        // -----------------
        targetAddDecisionSystem.Update(); // Add all signal targets
    }

    public void MasterTickThree ()
    {
        actionManagerTickSystem.Update();
        // ----------------
        influenceMapSystem.Update();
    }
}