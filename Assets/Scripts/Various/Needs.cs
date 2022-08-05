using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UtilityAI;

[Serializable]
public struct NeedTags
{
    public int Water;
    public int Food;

    public int Sum ()
    {
        return Water + Food;
    }

    public NeedTags TruncateNegative ()
    {
        return new NeedTags {
                   Water = Mathf.Max(Water, 0),
                   Food = Mathf.Max(Food, 0),
        };
    }

    public static NeedTags operator + (NeedTags x, NeedTags y)
    {
        return new NeedTags {
                   Water = x.Water + y.Water,
                   Food = x.Food + y.Food,
        };
    }

    public static NeedTags operator - (NeedTags x, NeedTags y)
    {
        return new NeedTags {
                   Water = x.Water - y.Water,
                   Food = x.Food - y.Food,
        };
    }
}

public struct NeedsData : IComponentData
{
    public NeedTags Value;
}

public static class CustomConsideration
{
    // =================================================================================
    [BurstCompile]
    public struct ConsiderationTargetNeedsTagJob : IJob
    {
        [ReadOnly] public ComponentDataFromEntity<NeedsData> needsdatas;
        [ReadOnly] public NativeArray<ConsiderationData> datas;

        [NativeDisableContainerSafetyRestriction][WriteOnly] public NativeArray<ConsiderationScore> scores;

        public int Length;

        public void Execute ()
        {
            for (int i = 0; i != datas.Length; i++) {
                var self = needsdatas[datas[i].Self].Value;
                var selfSum = self.Sum();

                if (selfSum == 0) {
                    scores[i] = new ConsiderationScore { Value = 0f };
                    continue;
                }

                var target = needsdatas[datas[i].Target].Value;

                // if (self.Food != 0 && target.Food < 0) return (-target.Food) / self.Food; // (float)(self.Food + target.Food) / self.Food;
                // if (self.Water != 0 && target.Water < 0) return (-target.Water) / self.Water; // (float)(self.Water + target.Water) / self.Water;
                // return 0f;
                var score = 1f - ((float)(self + target).TruncateNegative().Sum() / selfSum) * 0.5f; // WRONG

                // Debug.Log($"has Tag? {self} + {target} = {self.HasFlag(target)}");

                scores[i] = new ConsiderationScore { Value = score };
            }
        }
    }

    [Consideration(9, "Target_Needs", ParametersType.Boolean)]
    public static JobHandle TargetNeedsTag (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref NativeArray<ConsiderationScore> scores)
    {
        var needsdatas = mgr.GetComponentDataFromEntity<NeedsData>(true);

        return new ConsiderationTargetNeedsTagJob {
                   needsdatas = needsdatas,
                   datas = datas,
                   scores = scores
        }.Schedule(last);
    }
}