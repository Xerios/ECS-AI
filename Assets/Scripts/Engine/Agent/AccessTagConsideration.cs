using Game;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UtilityAI;

public static partial class CustomConsiderations
{
    [BurstCompile]
    public struct ConsiderationTargetAccessTagJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataFromEntity<AccessTagData> accesstagdatas;

        [ReadOnly] public NativeArray<ConsiderationData> datas;
        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

        public void Execute (int i)
        {
            var self = accesstagdatas[datas[i].Self].Value;
            var target = accesstagdatas[datas[i].Target].Value;

            // Debug.Log($"has Tag? {self} + {target} == {(target & self)}");

            float score = ((target & self) == self) ? 1f : 0f;

            scores[i] = new ConsiderationScore { Value = score };
        }
    }

    [Consideration(6, "Target_AccessTag_All", ParametersType.Boolean)]
    public static JobHandle TargetAccessTag (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref NativeArray<ConsiderationScore> scores)
    {
        var accesstagdatas = mgr.GetComponentDataFromEntity<AccessTagData>(true);

        return new ConsiderationTargetAccessTagJob {
                   accesstagdatas = accesstagdatas,
                   datas = datas,
                   scores = scores
        }.Schedule(datas.Length, 64, last);
    }
}