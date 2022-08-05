using Engine;
using Game;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class DatastoreDecisionsSystem : ComponentSystem
    {
        private DecisionWeightCalculateSystem calculateWeightSystem;

        public List<DecisionId> decisionIds = new List<DecisionId>(10);
        public NativeMultiHashMap<uint, short> tag2decisionIds = new NativeMultiHashMap<uint, short>(10, Allocator.Persistent);
        public Dictionary<short, Decision> decisions = new Dictionary<short, Decision>(10);
        public NativeMultiHashMap<short, ConsiderationParams> considerations = new NativeMultiHashMap<short, ConsiderationParams>(10, Allocator.Persistent);
        public NativeHashMap<short, int> considerationCount = new NativeHashMap<short, int>(10, Allocator.Persistent);
        public Dictionary<string, short> decisionsNameToId = new Dictionary<string, short>(10);

        protected override void OnCreateManager ()
        {
            calculateWeightSystem = World.GetExistingSystem<DecisionWeightCalculateSystem>();
        }

        public void AddDecision (Decision dse)
        {
            if (decisionIds.Contains(new DecisionId { Id = dse.Id })) return;

            decisionIds.Add(new DecisionId { Id = dse.Id });
            decisions[dse.Id] = dse;
            for (int i = 0; i < dse.Considerations.Length; i++) {
                considerations.Add(dse.Id, dse.Considerations[i]);
            }
            considerationCount.TryAdd(dse.Id, dse.Considerations.Length);

            decisionsNameToId[dse.Name] = dse.Id;
            calculateWeightSystem.decisionWeights[dse.Id] = dse.Weight;
            tag2decisionIds.Add(dse.Tags, dse.Id);
        }

        protected override void OnUpdate ()
        {}

        protected override void OnDestroyManager ()
        {
            tag2decisionIds.Dispose();
            considerations.Dispose();
            considerationCount.Dispose();
        }
    }
}