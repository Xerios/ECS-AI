using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class DecisionWeightCalculateSystem : JobComponentSystem
    {
        public NativeArray<float> decisionWeights;

        protected override void OnCreateManager ()
        {
            decisionWeights = new NativeArray<float>(64, Allocator.Persistent);
        }

        [BurstCompile]
        [ExcludeComponent(typeof(DecisionFailed), typeof(DecisionForget))]
        public struct CalcWeightJob : IJobForEach<DecisionId, DecisionTarget, DecisionPreferred, DecisionWeight>
        {
            [ReadOnly] internal NativeArray<float> dse_weights;

            private const float CURRENT_BONUS = 0.25f;

            public void Execute ([ReadOnly] ref DecisionId dseId, [ReadOnly] ref DecisionTarget target, [ReadOnly] ref DecisionPreferred preferred, ref DecisionWeight weight)
            {
                DecisionFlags flags = target.Flags;

                bool shouldOverride = (flags & DecisionFlags.OVERRIDE) != 0;
                float modWeight = (flags & DecisionFlags.DOUBLE_WEIGHT) != 0 ? 2f : 1f;

                float decisionBonus = Mathf.Clamp01(preferred.Value / 50f) * CURRENT_BONUS;
                float dseWeight = dse_weights[dseId.Id];

                // shouldOverride missing
                weight.Value = (modWeight * dseWeight) + decisionBonus;
            }
        }
        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            return new CalcWeightJob {
                       dse_weights = decisionWeights,
            }.Schedule(this, inputDeps);
        }

        protected override void OnDestroyManager ()
        {
            if (decisionWeights.IsCreated) decisionWeights.Dispose();
        }
    }
}