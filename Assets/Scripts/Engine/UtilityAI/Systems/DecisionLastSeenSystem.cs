using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(DecisionPreferenceSystem))]
    public class DecisionLastSeenSystem : JobComponentSystem
    {
        private UtilityAIGroup1 systemGroup;

        protected override void OnCreateManager ()
        {
            systemGroup = World.GetExistingSystem<UtilityAIGroup1>();
        }

        [ExcludeComponent(typeof(DecisionForget))]
        public struct LastSeenDecJob : IJobForEachWithEntity<DecisionLastSeen>
        {
            [ReadOnly] public float time;

            public EntityCommandBuffer.Concurrent cmds;

            public void Execute (Entity entity, int index, [ReadOnly] ref DecisionLastSeen lastSeen)
            {
                if (time > lastSeen.Value) {
                    cmds.AddComponent<DecisionForget>(index, entity, new DecisionForget());
                }
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var job = new LastSeenDecJob {
                cmds = systemGroup.PostUpdateCommands.ToConcurrent(),
                time = UnityEngine.Time.time
            }.Schedule(this, inputDeps);

            job.Complete();

            return job;
        }
    }
}