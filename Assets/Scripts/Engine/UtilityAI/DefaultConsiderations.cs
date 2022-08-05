using Game;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    public static class DefaultConsiderations
    {
        [Consideration(1, "Cooldown", ParametersType.Value)]
        public static JobHandle CooldownScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas,
            ref NativeArray<ConsiderationScore> scores)
        {
            throw new NotImplementedException("Look for entity data");
            // float value = (selfEntityContext.GetMind().History.GetEventDuration(context.GetDecisionHistory(), Time.time - parameters.Min) / parameters.Min);

            // return value;
        }


        [Consideration(5, "Target_Score", ParametersType.None)]
        public static JobHandle TargetScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref NativeArray<ConsiderationScore> scores)
        {
            last.Complete();
            for (int i = 0; i != datas.Length; i++) {
                var self = enMgr.GetSharedComponentData<ScoreEvaluate>(datas[i].Target).Evaluate;

                scores[i] = new ConsiderationScore { Value = self.Invoke(datas[i].Self) };
            }
            return default(JobHandle);
        }
        // =================================================================================

        [BurstCompile]
        public struct ConsiderationRepeatJob : IJobParallelFor
        {
            [ReadOnly] public BufferFromEntity<DecisionHistoryRecord> records;
            [ReadOnly] public ComponentDataFromEntity<ActiveDecision> actives;
            [ReadOnly] public ComponentDataFromEntity<MindReference> mindrefs;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public float time;

            public void Execute (int i)
            {
                // if (!actives.Exists(mindrefs[datas[i].Self].Value)) {
                //     scores[i] = new ConsiderationScore { Value = 2f };
                //     return;
                // }

                if (!records.Exists(mindrefs[datas[i].Self].Value)) {
                    scores[i] = new ConsiderationScore { Value = 2f };
                    return;
                }

                int repeats = 0;
                var buffer = records[mindrefs[datas[i].Self].Value];

                if (buffer.Length != 0) {
                    var beforeTime = time - datas[i].Value.Max;

                    for (int j = 0; j < buffer.Length; j++) {
                        var item = buffer[j];
                        if (item.EndTime < beforeTime) break;

                        // Old way of doing it, might be wrong
                        // If (item.EndTime < beforeTime || item.StartTime < beforeTime) break;

                        // (item != LastEvent) => Ignore current event ( not counted as a repeat )
                        if (item.dseId.Equals(datas[i].dseId) && item.target.Equals(datas[i].Target) &&
                            (j != 0 || (j == 0 && datas[i].dseId != actives[mindrefs[datas[i].Self].Value].dseId && datas[i].Target != actives[mindrefs[datas[i].Self].Value].target))) repeats++;
                    }
                }

                float score = (repeats == 0) ? 0f : ((float)repeats / datas[i].Value.Min);

                scores[i] = new ConsiderationScore { Value = score };
            }
        }

        [Consideration(3, "Repeats", ParametersType.Range)]
        public static JobHandle RepeatsScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref NativeArray<ConsiderationScore> scores)
        {
            var records = mgr.GetBufferFromEntity<DecisionHistoryRecord>(true);
            var actives = mgr.GetComponentDataFromEntity<ActiveDecision>(true);
            var mindrefs = mgr.GetComponentDataFromEntity<MindReference>(true);

            return new ConsiderationRepeatJob {
                       records = records,
                       datas = datas,
                       scores = scores,
                       actives = actives,
                       mindrefs = mindrefs,
                       time = time,
            }.Schedule(datas.Length, 64, last);
        }

        // =================================================================================
        [BurstCompile]
        public struct ConsiderationTargetDistanceJob : IJobParallelFor
        {
            [ReadOnly] public ComponentDataFromEntity<SignalPosition> positions;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public void Execute (int i)
            {
                if (!positions.Exists(datas[i].Self) || !positions.Exists(datas[i].Target)) {
                    scores[i] = new ConsiderationScore { Value = 2f };
                    return;
                }
                var from = positions[datas[i].Self].Value;
                var to = positions[datas[i].Target].Value;
                var dist = math.distance(from, to);

                float minSqr = (datas[i].Value.Min * datas[i].Value.Min);
                float maxSqr = (datas[i].Value.Max * datas[i].Value.Max);

                float score = (dist - minSqr) / (maxSqr - minSqr);

                scores[i] = new ConsiderationScore { Value = score };
            }
        }

        [Consideration(4, "Target_Distance", ParametersType.Range)]
        public static JobHandle TargetDistanceScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref
            NativeArray<ConsiderationScore> scores)
        {
            var positions = mgr.GetComponentDataFromEntity<SignalPosition>(true);

            return new ConsiderationTargetDistanceJob {
                       positions = positions,
                       datas = datas,
                       scores = scores
            }.Schedule(datas.Length, 64, last);
        }
        // =================================================================================
        [BurstCompile]
        public struct ConsiderationHealthJob : IJobParallelFor
        {
            [ReadOnly] public ComponentDataFromEntity<HealthStateData> health;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public void Execute (int i)
            {
                scores[i] = new ConsiderationScore { Value = health[datas[i].Target].health / health[datas[i].Target].maxHealth };
            }
        }

        [Consideration(7, "Target_Health", ParametersType.None)]
        public static JobHandle TargetHealthScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref
            NativeArray<ConsiderationScore> scores)
        {
            var hp = mgr.GetComponentDataFromEntity<HealthStateData>(true);

            return new ConsiderationHealthJob {
                       health = hp,
                       datas = datas,
                       scores = scores
            }.Schedule(datas.Length, 64, last);
        }
        // =================================================================================
        [BurstCompile]
        public struct ConsiderationHealthAliveJob : IJobParallelFor
        {
            [ReadOnly] public ComponentDataFromEntity<HealthStateData> health;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public void Execute (int i)
            {
                scores[i] = new ConsiderationScore { Value = health[datas[i].Target].health == 0f ? 0f : 1f };
            }
        }

        [Consideration(8, "Target_IsAlive", ParametersType.Boolean)]
        public static JobHandle TargetHealthAliveScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref
            NativeArray<ConsiderationScore> scores)
        {
            var hp = mgr.GetComponentDataFromEntity<HealthStateData>(true);

            return new ConsiderationHealthAliveJob {
                       health = hp,
                       datas = datas,
                       scores = scores
            }.Schedule(datas.Length, 64, last);
        }
        // =================================================================================
        [BurstCompile]
        public struct ConsiderationStaminaJob : IJobParallelFor
        {
            [ReadOnly] public ComponentDataFromEntity<StaminaData> stamina;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public void Execute (int i)
            {
                scores[i] = new ConsiderationScore { Value = stamina[datas[i].Self].Value / stamina[datas[i].Self].Max };
            }
        }

        [Consideration(9, "Stamina", ParametersType.None)]
        public static JobHandle StaminaScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref
            NativeArray<ConsiderationScore> scores)
        {
            var sta = mgr.GetComponentDataFromEntity<StaminaData>(true);

            return new ConsiderationStaminaJob {
                       stamina = sta,
                       datas = datas,
                       scores = scores
            }.Schedule(datas.Length, 64, last);
        }
        // =================================================================================
        [BurstCompile]
        public struct ConsiderationAssignmentJob : IJobParallelFor
        {
            [ReadOnly] public ComponentDataFromEntity<AssignmentTypeData> assignment;
            [ReadOnly] public ComponentDataFromEntity<AssignmentEntityData> assignmentEntity;
            [ReadOnly] public ComponentDataFromEntity<SignalPosition> positions;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public void Execute (int i)
            {
                if (assignmentEntity[datas[i].Self].AssignedEntity == Entity.Null) {
                    scores[i] = new ConsiderationScore { Value = 0f };
                    return;
                }
                if (!positions.Exists(assignmentEntity[datas[i].Self].AssignedEntity)) {
                    scores[i] = new ConsiderationScore { Value = 0f };
                    return;
                }
                scores[i] = new ConsiderationScore { Value = (datas[i].Value.Property & assignment[datas[i].Self].TypeId) != 0 ? 1.0f : 0.0f };
            }
        }

        [Consideration(9, "HasAssignment", ParametersType.Ability)]
        public static JobHandle AssignmentScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref
            NativeArray<ConsiderationScore> scores)
        {
            var assignment = mgr.GetComponentDataFromEntity<AssignmentTypeData>(true);
            var assignmentEntity = mgr.GetComponentDataFromEntity<AssignmentEntityData>(true);
            var positions = mgr.GetComponentDataFromEntity<SignalPosition>(true);

            return new ConsiderationAssignmentJob {
                       assignment = assignment,
                       assignmentEntity = assignmentEntity,
                       positions = positions,
                       datas = datas,
                       scores = scores
            }.Schedule(datas.Length, 64, last);
        }
        // =================================================================================
        [BurstCompile]
        public struct ConsiderationIsEngagedJob : IJobParallelFor
        {
            [ReadOnly] public ComponentDataFromEntity<SpatialDetectionTimer> timers;

            [ReadOnly] public NativeArray<ConsiderationData> datas;
            [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

            public void Execute (int i)
            {
                scores[i] = new ConsiderationScore { Value = timers[datas[i].Self].timer > 1f ? 1.0f : 0.0f };
            }
        }

        [Consideration(9, "IsEngaged", ParametersType.Boolean)]
        public static JobHandle EngagedScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref
            NativeArray<ConsiderationScore> scores)
        {
            var timers = mgr.GetComponentDataFromEntity<SpatialDetectionTimer>(true);

            return new ConsiderationIsEngagedJob {
                       timers = timers,
                       datas = datas,
                       scores = scores
            }.Schedule(datas.Length, 64, last);
        }
    }
}