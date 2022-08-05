using Engine;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UtilityAI;

[InternalBufferCapacity(0)]
public struct InfluenceMapData : IBufferElementData
{
    public float Value;

    public static implicit operator InfluenceMapData (float d) => (new InfluenceMapData { Value = d });

    public static InfluenceMapData operator + (InfluenceMapData a, InfluenceMapData b) => (new InfluenceMapData { Value = a.Value + b.Value });
    public static InfluenceMapData operator * (InfluenceMapData a, InfluenceMapData b) => (new InfluenceMapData { Value = a.Value * b.Value });
}

public struct InfluenceMapUpdate : IComponentData {}
public struct InfluenceMap_UpdateOnce : IComponentData {}

public struct InfluenceMap_ClearData : IComponentData {}
public struct InfluenceMap_BlurData : IComponentData {}
public struct InfluenceMap_NormalizeData : IComponentData {}

public struct InfluenceMap_DegradeData : IComponentData
{
    public float Value;
}

[InternalBufferCapacity(0)]
public struct InfluenceMapToAddData : IBufferElementData
{
    public int2 pos;
    public float weight;
    public byte size;

    public InfluenceMapToAddData(int2 pos, float weight, byte size)
    {
        this.pos = pos;
        this.weight = weight;
        this.size = size;
    }

    public static void AddData (DynamicBuffer<InfluenceMapToAddData> buffer, int2 pos, float weight, byte size)
    {
        InfluenceMapToAddData e;

        e.pos = pos;
        e.weight = weight;
        e.size = size;

        // if (buffer.Length == buffer.Capacity) {
        // Debug.LogWarning("InfluenceMapToAddData buffer full.");
        // return;
        // }

        buffer.Add(e);
    }
}

public struct InfluenceMap_AddUnits_Data : IComponentData
{
    public byte FactionLayerFlags;
}

public struct InfluenceMap_FilterUnits_Data : IComponentData
{
    public byte FactionLayerFlags;
}

public struct InfluenceMap_CopyFrom : IComponentData
{
    public Entity Value;
}

public struct InfluenceMap_Circle : IComponentData
{
    public int Radius;
    public float Value;
}

public struct InfluenceMap_AddNavMesh_Data : IComponentData {}