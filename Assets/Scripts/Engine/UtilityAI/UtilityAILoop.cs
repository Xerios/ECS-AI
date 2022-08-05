using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilityAIGroup1))]
    public class UtilityAIGroup0 : ComponentSystemGroup {}

    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilityAIGroup2))]
    public class UtilityAIGroup1 : ComponentSystemGroup {}

    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilityAIGroupLifecyle))]
    public class UtilityAIGroup2 : ComponentSystemGroup {}

    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilityAIGroupCalculation))]
    public class UtilityAIGroupLifecyle : ComponentSystemGroup {}

    [DisableAutoCreation]
    // [UpdateAfter(typeof(UtilityAILifeCycleBarrier))]
    public class UtilityAIGroupCalculation : ComponentSystemGroup {}

    [DisableAutoCreation]
    [UpdateBefore(typeof(ConsiderationDestroyDeadSystem))]
    public class UtilityAILifeCycleBarrier : EntityCommandBufferSystem  {}

    [DisableAutoCreation]
    [UpdateAfter(typeof(ConsiderationDestroyDeadSystem))]
    public class UtilityAILifeCycleAfterDestroyBarrier : EntityCommandBufferSystem  {}

    [DisableAutoCreation]
    [UpdateAfter(typeof(DecisionTargetForgetSystem))]
    public class UtilityAIForgetBarrier : EntityCommandBufferSystem  {}


    public class UtilityAILoop
    {
        public static UtilityAILoop Instance;

        public EntityManager mgr;

        private DecisionHistorySystem decisionHistorySystem;
        public DatastoreDecisionsSystem calculateDecisionsSystem; // public because some stuff is used outside

        private UtilityAIGroup0 group0;
        private UtilityAIGroup1 group1;
        private UtilityAIGroup2 group2;
        private UtilityAIGroupLifecyle groupLifeCycle;
        private UtilityAIGroupCalculation group4;

        public UtilityAILoop()
        {
            World world = World.Active;

            Instance = this;

            mgr = world.EntityManager;

            group0 = world.CreateSystem<UtilityAIGroup0>();

            {
#if UNITY_EDITOR
                group0.AddSystemToUpdateList(world.CreateSystem<DecisionClearDebugSystem>());
#endif
                {
                    group1 = world.CreateSystem<UtilityAIGroup1>();
                    group1.AddSystemToUpdateList(world.CreateSystem<DecisionLastSeenSystem>());
                    group1.AddSystemToUpdateList(world.CreateSystem<DecisionPreferenceSystem>());
                    group1.AddSystemToUpdateList(world.CreateSystem<DecisionFailedSystem>());
                    group1.AddSystemToUpdateList(world.CreateSystem<DecisionWeightCalculateSystem>());
                    group0.AddSystemToUpdateList(group1);
                }

                {
                    var barrier = world.CreateSystem<UtilityAIForgetBarrier>();
                    group2 = world.CreateSystem<UtilityAIGroup2>();
                    group2.AddSystemToUpdateList(world.CreateSystem<DecisionTargetForgetSystem>());
                    group2.AddSystemToUpdateList(barrier);
                    group0.AddSystemToUpdateList(group2);
                }

                {
                    var barrier = world.CreateSystem<UtilityAILifeCycleBarrier>();
                    var barrier2 = world.CreateSystem<UtilityAILifeCycleAfterDestroyBarrier>();

                    groupLifeCycle = world.CreateSystem<UtilityAIGroupLifecyle>();
                    groupLifeCycle.AddSystemToUpdateList(world.CreateSystem<MindDestroySystem>());
                    groupLifeCycle.AddSystemToUpdateList(world.CreateSystem<DecisionAddSystem>());
                    groupLifeCycle.AddSystemToUpdateList(world.CreateSystem<DecisionRemoveSystem>());
                    groupLifeCycle.AddSystemToUpdateList(barrier);
                    groupLifeCycle.AddSystemToUpdateList(world.CreateSystem<ConsiderationDestroyDeadSystem>());
                    groupLifeCycle.AddSystemToUpdateList(barrier2);
                    group0.AddSystemToUpdateList(groupLifeCycle);
                }

                {
                    group4 = world.CreateSystem<UtilityAIGroupCalculation>();
                    group4.AddSystemToUpdateList(world.CreateSystem<DatastoreDecisionsSystem>());
                    group4.AddSystemToUpdateList(world.CreateSystem<ConsiderationCalculateSystem>());
                    group4.AddSystemToUpdateList(world.CreateSystem<DecisionBuildSystem>());
                    group4.AddSystemToUpdateList(world.CreateSystem<SelectBestDecisionSystem>());
                    group0.AddSystemToUpdateList(group4);
                }
            }

            decisionHistorySystem = world.GetOrCreateSystem<DecisionHistorySystem>();

            calculateDecisionsSystem = world.GetExistingSystem<DatastoreDecisionsSystem>();
        }

        public void Tick ()
        {
            group0.Update();
        }

        public void AddDecision (Decision dse)
        {
            calculateDecisionsSystem.AddDecision(dse);
        }

        public short GetDecisionId (string name)
        {
            short dseId = -1;

            calculateDecisionsSystem.decisionsNameToId.TryGetValue(name, out dseId);
            return dseId;
        }

        public string GetDecisionName (short id)
        {
            Decision dse;

            calculateDecisionsSystem.decisions.TryGetValue(id, out dse);
            return dse.Name;
        }
    }
}