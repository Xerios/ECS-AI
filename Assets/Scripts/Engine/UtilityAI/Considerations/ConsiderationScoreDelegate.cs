using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace UtilityAI
{
    public delegate JobHandle ConsiderationScoreDelegate (JobHandle last, JobComponentSystem mgr, EntityManager enMgr, float time, NativeArray<ConsiderationData> dataArray,
        ref NativeArray<ConsiderationScore> score);
}