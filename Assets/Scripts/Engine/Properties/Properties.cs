using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UtilityAI;

public static partial class CustomConsiderations
{
    [Consideration(0, "Boolean", ParametersType.Property | ParametersType.Boolean)]
    public static JobHandle BooleanScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref NativeArray<ConsiderationScore> scores)
    {
        throw new NotImplementedException("Boolean score not setup");
        return default(JobHandle);
    }

    [Consideration(2, "Property", ParametersType.Property)]
    public static JobHandle PropertyScore (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> datas, ref NativeArray<ConsiderationScore> scores)
    {
        // for (int i = 0; i != datas.Length; i++) {
        //     var self = enMgr.GetSharedComponentData<PropertiesSelf>(datas[i].Self).Value;

        //     float score = self.GetNormalizedValue((PropertyType)datas[i].Value.Property);

        //     scores[i] = new ConsiderationScore { Value = score };
        // }
        throw new NotImplementedException("Boolean score not setup");
        return default(JobHandle);
    }
}