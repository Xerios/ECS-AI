using Game;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(DecisionBuildSystem))]
    public class SelectBestDecisionSystem : JobComponentSystem
    {
        [Unity.Burst.BurstCompile]
        [RequireComponentTag(typeof(SelectBestDecision), typeof(DecisionOption))]
        public struct SelectBestJob : IJobForEachWithEntity<BestDecision>
        {
            [ReadOnly] public BufferFromEntity<DecisionOption> options;

            public void Execute (Entity entity, int index, ref BestDecision best)
            {
                var list = options[entity];

                int bestItemId = -1;
                float bestScore = float.NegativeInfinity;

                for (int j = 0; j != list.Length; j++) {
                    if (bestScore < list[j].Score) {
                        bestScore = list[j].Score;
                        bestItemId = j;
                    }
                }
                if (bestItemId == -1) {
                    best.data = new DecisionContext(0, false, Entity.Null, Entity.Null);
                    return;
                }
                best.data = list[bestItemId].GetContext();
            }
        }


        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            return new SelectBestJob {
                       options = GetBufferFromEntity<DecisionOption>(true)
            }.Schedule(this, inputDeps);
        }
    }
}