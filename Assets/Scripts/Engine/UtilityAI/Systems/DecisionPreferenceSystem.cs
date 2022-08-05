using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(DecisionFailedSystem))]
    public class DecisionPreferenceSystem : JobComponentSystem
    {
        [BurstCompile]
        public struct AddActivePreferenceJob : IJobForEach<ActiveDecision>
        {
            public float time;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<DecisionLastSeen> lastSeen;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<DecisionPreferred> preferred;

            public void Execute ([ReadOnly] ref ActiveDecision active)
            {
                if (active.entity.Equals(Entity.Null)) return;
                if (!preferred.Exists(active.entity)) return;

                preferred[active.entity] = new DecisionPreferred { Value = preferred[active.entity].Value + 1f };

                if (!lastSeen.Exists(active.entity)) return;
                lastSeen[active.entity] = new DecisionLastSeen { Value = time + 10f }; // Used so that DecisionLastSeenSystem doesn't remove running decisions
            }
        }

        [BurstCompile]
        public struct DecayPreferenceJob : IJobForEach<DecisionPreferred>
        {
            public void Execute (ref DecisionPreferred preferred)
            {
                preferred.Value *= 0.99f;
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var decayJob = new DecayPreferenceJob().Schedule(this, inputDeps);
            var preferenceJob = new AddActivePreferenceJob {
                time = Time.time,
                lastSeen = GetComponentDataFromEntity<DecisionLastSeen>(false),
                preferred = GetComponentDataFromEntity<DecisionPreferred>(false)
            }.Schedule(this, decayJob);

            return preferenceJob;
        }
    }
}