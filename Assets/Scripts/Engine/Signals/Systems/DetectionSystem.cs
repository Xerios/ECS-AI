using Engine;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class DetectionSystem : JobComponentSystem
    {
        [Unity.Burst.BurstCompile]
        [RequireComponentTag(typeof(EntityWithFactionPosition))]
        public struct RaiseDetectionTimerJob : IJobForEachWithEntity<AgentFaction, SpatialDetectionSpeed, SpatialDetectionTimer>
        {
            public float dt;
            [ReadOnly] public BufferFromEntity<EntityWithFactionPosition> buffer;

            public void Execute (Entity entity, int index, [ReadOnly] ref AgentFaction faction, [ReadOnly] ref SpatialDetectionSpeed speed, ref SpatialDetectionTimer timer)
            {
                var list = buffer[entity];

                float reduce = 1f;

                for (int j = 0; j != list.Length; j++) {
                    if ((list[j].faction & faction.LayerFlags) != faction.LayerFlags) {
                        timer.timer += dt * speed.speed * reduce;
                        reduce *= 0.5f;
                    }
                }

                timer.timer = math.min(timer.timer, SpatialDetectionTimer.MAX_LIMIT);
            }
        }

        [Unity.Burst.BurstCompile]
        public struct LowerDetectionTimerJob : IJobForEach<SpatialDetectionTimer>
        {
            public float dt;

            public void Execute (ref SpatialDetectionTimer timer)
            {
                timer.timer = math.max(0, timer.timer - dt);
            }
        }

        [Unity.Burst.BurstCompile]
        public struct AdaptDetectionStateJob : IJobForEach<SpatialDetectionTimer, SpatialDetectionState>
        {
            public void Execute ([ReadOnly] ref SpatialDetectionTimer timer, ref SpatialDetectionState state)
            {
                if (timer.timer <= SpatialDetectionTimer.DEFEND) {
                    state.state = SpatialDetectionState.NORMAL;
                }
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            var jobA = new LowerDetectionTimerJob {
                dt = AIManager.TICK_RATE * Time.timeScale
            }.Schedule(this, inputDeps);

            var jobB = new RaiseDetectionTimerJob {
                dt = AIManager.TICK_RATE * Time.timeScale,
                buffer = GetBufferFromEntity<EntityWithFactionPosition>(true)
            }.Schedule(this, jobA);

            var jobC = new AdaptDetectionStateJob().Schedule(this, jobB);

            jobC.Complete();

            return default;
        }
    }
}