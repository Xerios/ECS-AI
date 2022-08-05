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
    [AlwaysUpdateSystem] // Otherwise doesn't when no considerations are in place ( prob because of m_Group)
    [UpdateBefore(typeof(DecisionBuildSystem))]
    public class ConsiderationCalculateSystem : JobComponentSystem
    {
        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(ConsiderationDecisionParent)),
                    ComponentType.ReadOnly(typeof(ConsiderationType)),
                    ComponentType.ReadOnly(typeof(ConsiderationData)),
                    typeof(ConsiderationScore)
                    );
        }


        private JobHandle Calculate (JobHandle handle = default)
        {
            var time = Time.time;

            var em = EntityManager;

            UnityEngine.Profiling.Profiler.BeginSample("Calculate");

            var chunks = m_Group.CreateArchetypeChunkArray(Allocator.TempJob);

            var chunkTypeType = GetArchetypeChunkSharedComponentType<ConsiderationType>();

            var chunkDataType = GetArchetypeChunkComponentType<ConsiderationData>(isReadOnly: true);
            var chunkScoreType = GetArchetypeChunkComponentType<ConsiderationScore>(isReadOnly: false);

            var jobHandles = new NativeArray<JobHandle>(chunks.Length, Allocator.Temp);

            for (var i = 0; i < chunks.Length; i++) {
                var chunk = chunks[i];
                var considType = chunk.GetSharedComponentData(chunkTypeType, em);
                var data = chunk.GetNativeArray(chunkDataType);
                var scores = chunk.GetNativeArray(chunkScoreType);

                // UnityEngine.Profiling.Profiler.BeginSample($"Chunk {considType.DataType} = {data.Length}");
                jobHandles[i] = ConsiderationMap.Get(considType.DataType)(handle, this, em, time, data, ref scores);
                // UnityEngine.Profiling.Profiler.EndSample();
            }
            chunks.Dispose();
            var resultDeps = JobHandle.CombineDependencies(jobHandles);

            UnityEngine.Profiling.Profiler.EndSample();
            return resultDeps;
        }


        [BurstCompile]
        [ExcludeComponent(typeof(DecisionFailed), typeof(DecisionForget))]
        struct SetupDecisionWeightJob : IJobForEach<DecisionWeight, DecisionScore>
        {
            public void Execute ([ReadOnly] ref DecisionWeight weight, ref DecisionScore score)
            {
                score.Value = weight.Value;
            }
        }

        [BurstCompile]
        struct CalcCurveJob : IJobForEach<ConsiderationCurve, ConsiderationScore>
        {
            public void Execute ([ReadOnly] ref ConsiderationCurve curveData, ref ConsiderationScore score)
            {
                score.Value = math.saturate(curveData.UtilCurve.Evaluate(score.Value));
            }
        }

        [BurstCompile]
        struct CalcMergeJob : IJobForEach<ConsiderationDecisionParent, ConsiderationModfactor, ConsiderationScore>
        {
            public ComponentDataFromEntity<DecisionScore> decisionScores;

            public void Execute ([ReadOnly] ref ConsiderationDecisionParent parent, [ReadOnly] ref ConsiderationModfactor modfactor, [ReadOnly] ref ConsiderationScore score)
            {
                // -------------- Compensation factor
                float makeUpValue = (1f - score.Value) * modfactor.Value;
                float finalConsiderationValue = score.Value + (makeUpValue * score.Value);

                float finalScore = decisionScores[parent.Value].Value;

                finalScore *= (finalConsiderationValue); // removed math.saturate, will it change anything?

                decisionScores[parent.Value] = new DecisionScore { Value = finalScore };
            }
        }

        [BurstCompile]
        public struct DebugConsiderationsJob : IJobForEachWithEntity<ConsiderationMindParent, ConsiderationDecisionParent>
        {
            [WriteOnly] public BufferFromEntity<DebugConsideration> options;

            public void Execute (Entity entity, int index, [ReadOnly] ref ConsiderationMindParent mind, [ReadOnly] ref ConsiderationDecisionParent parent)
            {
                if (!options.Exists(mind.Value)) return;

                options[mind.Value].Add(new DebugConsideration {
                        considerationEntity = entity,
                        decisionEntity = parent.Value,
                    });
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            // -----------------------------
            var calculateJobs = Calculate(inputDeps);
            // -----------------------------
            var resetWeight = new SetupDecisionWeightJob().Schedule(this, calculateJobs);
            var jobCurve = new CalcCurveJob().Schedule(this, resetWeight);
            // -----------------------------
            var jobMerge = new CalcMergeJob
            {
                decisionScores = GetComponentDataFromEntity<DecisionScore>(false)
            }.ScheduleSingle(this, jobCurve);

#if UNITY_EDITOR
            var jobDebug = new DebugConsiderationsJob {
                options = GetBufferFromEntity<DebugConsideration>()
            }.ScheduleSingle(this, jobMerge);
            jobDebug.Complete();
#endif

            return jobMerge;// default(JobHandle);
        }
    }
}