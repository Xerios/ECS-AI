using Engine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    [UpdateBefore(typeof(AIManager))]
    [UpdateAfter(typeof(DecisionHistorySystem))]
    public class MindUpdateSystem : ComponentSystem
    {
        public struct MindsetUpdateEntry
        {
            public Entity belongsTo, target;
            public int addOrRemove;
            public Mindset mindset;

            public MindsetUpdateEntry(Entity belongsTo, Entity entity, int addOrRemove, Mindset mindset) : this()
            {
                this.belongsTo = belongsTo;
                this.target = entity;
                this.addOrRemove = addOrRemove;
                this.mindset = mindset;
            }
        }

        public Queue<MindsetUpdateEntry> mindsetQueue;

        protected override void OnCreateManager ()
        {
            mindsetQueue = new Queue<MindsetUpdateEntry>();
        }

        public void Add (Entity belongsTo, Entity entity, Mindset mindset)
        {
            if (entity.Index < 0 || entity.Equals(Entity.Null)) {
                Debug.LogWarning("Cannot remove mindset, entity target is null!");
                return;
            }
            mindsetQueue.Enqueue(new MindsetUpdateEntry(belongsTo, entity, 1, mindset));
        }

        public void Remove (Entity belongsTo, Entity entity, Mindset mindset)
        {
            if (entity.Index < 0 || entity.Equals(Entity.Null)) {
                Debug.LogWarning("Cannot remove mindset, entity target is null!");
                return;
            }
            mindsetQueue.Enqueue(new MindsetUpdateEntry(belongsTo, entity, -1, mindset));
        }

        protected override void OnUpdate ()
        {
            while (mindsetQueue.Count > 0) {
                var current = mindsetQueue.Dequeue();

                if (current.addOrRemove == 1) {
                    foreach (var dse in current.mindset.DSEs) {
                        Mind.AddDecisionOption(current.belongsTo, current.target, PostUpdateCommands, dse);
                    }
                }else{
                    foreach (var dse in  current.mindset.DSEs) {
                        Mind.RemoveDecisionOptions(current.belongsTo, current.target, PostUpdateCommands, dse);
                    }
                }
            }
        }

        protected override void OnDestroyManager ()
        {
            // mindsetQueue.Dispose();
        }
    }
}