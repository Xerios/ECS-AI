using Engine;
using Game;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    [UpdateBefore(typeof(AIManager))]
    [UpdateAfter(typeof(MindInitSystem))]
    public class DecisionHistorySystem : JobComponentSystem
    {
        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(ComponentType.ReadOnly(typeof(ActiveDecision)), typeof(DecisionHistoryRecord));
        }

        public int Length;

        [BurstCompile]
        public struct RecordHistoryJob : IJobChunk
        {
            public float time;
            [ReadOnly] public ArchetypeChunkComponentType<ActiveDecision> activeType;
            public ArchetypeChunkBufferType<DecisionHistoryRecord> recordType;

            public void Execute (ArchetypeChunk chunk, int chunkIndex, int entityOffset)
            {
                var active = chunk.GetNativeArray(activeType);
                var record = chunk.GetBufferAccessor(recordType);

                for (int i = 0; i < chunk.Count; i++) {
                    if (record[i].Length == 0) {
                        record[i].Add(new DecisionHistoryRecord { dseId = active[i].dseId, target = active[i].target, StartTime = time, EndTime = time });
                        continue;
                    }

                    var last = record[i][0];

                    if (last.dseId == active[i].dseId && last.target == active[i].target) {
                        record[i].RemoveAt(0);
                        record[i].Insert(0, new DecisionHistoryRecord { dseId = active[i].dseId, target = active[i].target, StartTime = last.StartTime, EndTime = time });
                    }else{
                        if (record[i].Length == record[i].Capacity) {
                            record[i].RemoveAt(record[i].Capacity - 1);
                        }
                        record[i].Insert(0, new DecisionHistoryRecord { dseId = active[i].dseId, target = active[i].target, StartTime = time, EndTime = time });
                    }
                }
            }
        }

        protected override JobHandle OnUpdate (JobHandle inputDeps)
        {
            Length = m_Group.CalculateLength();

            var clearJob = new RecordHistoryJob {
                activeType = GetArchetypeChunkComponentType<ActiveDecision>(true),
                recordType = GetArchetypeChunkBufferType<DecisionHistoryRecord>(),
                time = Time.time,
            }.Schedule(m_Group, inputDeps);

            clearJob.Complete();

            return clearJob;
        }
    }
}