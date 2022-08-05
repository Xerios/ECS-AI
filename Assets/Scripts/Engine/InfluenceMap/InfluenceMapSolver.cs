using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UtilityAI;

namespace Engine
{
    public static class InfluenceMapSolver
    {
        public static void Solve (InfluenceMapBuilder builder)
        {
            Profiler.BeginSample("InfluenceMapBuild");
            var grid = builder.grid;

            // Debug.Log("Solve" + builder.metas.Count);
            while (builder.metas.Count > 0) {
                var meta = builder.metas.Dequeue();
                // Debug.Log("Solve:" + meta.type);

                switch (meta.type) {
                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.SetAllValues:
                    {
                        for (int i = 0; i < grid.Length; i++) {
                            grid[i] = meta.weight;
                        }
                        break;
                    }

                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.FallOff:
                    {
                        var distMaxSqr = meta.distance * meta.distance;
                        var fullDist = builder.LocalOffset * builder.LocalOffset;

                        for (int y = 0; y < builder.LocalSize; y++) {
                            for (int x = 0; x < builder.LocalSize; x++) {
                                var idx = GetLocalIndex(x, y, builder.LocalSize);

                                float2 pos = GetWorldPos(builder.gridMin + new int2(x, y));

                                float distSqr = math.distancesq(meta.pos, pos);

                                grid[idx] += meta.weight * (1f - math.clamp(math.pow(math.sqrt(distSqr) / fullDist, 4), 0, 1)); // -x^4 + 1
                            }
                        }
                        break;
                    }

                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.ReduceValuesAround:
                    {
                        var distMaxSqr = meta.distance * meta.distance;

                        for (int y = 0; y < builder.LocalSize; y++) {
                            for (int x = 0; x < builder.LocalSize; x++) {
                                var idx = GetLocalIndex(x, y, builder.LocalSize);

                                float2 pos = GetWorldPos(builder.gridMin + new int2(x, y));
                                float distSqr = math.distancesq(meta.pos, pos);

                                if (distSqr > distMaxSqr) {
                                    grid[idx] = 0;
                                    continue;
                                }

                                grid[idx] *= (1 - math.clamp(math.pow(math.sqrt(distSqr) / meta.distance, 4), 0, 1));  // -x^4 + 1
                            }
                        }
                        break;
                    }

                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.Map_AddAnd:
                        ActionWithMap(builder, grid, meta.map, meta.weight, PerfromMapAction_AddAnd);
                        break;

                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.Map_AddClamped:
                        ActionWithMap(builder, grid, meta.map, meta.weight, PerfromMapAction_AddClamped);
                        break;

                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.Map_AddInversedAnd:
                        ActionWithMap(builder, grid, meta.map, meta.weight, PerfromMapAction_AddInversedAnd);
                        break;

                    // --------------------------------------------------------------------------
                    case InfluenceBuilderMeta.Types.Map_AddNegClamped:
                        ActionWithMap(builder, grid, meta.map, meta.weight, PerfromMapAction_AddNegClamped);
                        break;
                }
            }

            // Get highest and lowest points
            // ------------------------------------------
            float localMimima = float.MaxValue;
            float localMaxima = float.MinValue;
            int localMinimaId = -1;
            int localMaximaId = -1;

            for (int i = 0; i < grid.Length; i++) {
                if (localMimima > grid[i]) {
                    localMimima = grid[i];
                    localMinimaId = i;
                    continue;
                }
                if (localMaxima < grid[i]) {
                    localMaxima = grid[i];
                    localMaximaId = i;
                    continue;
                }
            }

            var localMimimaPos = GetWorldPos(builder.gridMin + GetXY(localMinimaId, builder.LocalSize, builder.LocalScale));
            builder.lowestPosition = ToVector3X0Y(localMimimaPos);

            var localMaximaPos = GetWorldPos(builder.gridMin + GetXY(localMaximaId, builder.LocalSize, builder.LocalScale));
            builder.highestPosition = ToVector3X0Y(localMaximaPos);

            Profiler.EndSample();
        }

        // --------------------------------------------------------
        private static void ActionWithMap (InfluenceMapBuilder builder, NativeArray<float> grid, byte mapType, float modifier, PerfromMapAction action)
        {
            InfluenceMapSystem influenceMapSystem = Bootstrap.world.GetExistingSystem<InfluenceMapSystem>();
            var influenceMapEntity = influenceMapSystem.Get(mapType);
            var worldSize = influenceMapSystem.WorldSize;
            var worldOffset = influenceMapSystem.WorldOffset;

            var data = Bootstrap.world.EntityManager.GetBuffer<InfluenceMapData>(influenceMapEntity).Reinterpret<float>();

            for (int y = builder.gridMin.y; y < builder.gridMax.y; y++) {
                for (int x = builder.gridMin.x; x < builder.gridMax.x; x++) {
                    var idxLocal = (y - builder.gridMin.y) * builder.LocalSize + (x - builder.gridMin.x);

                    var idx = (math.clamp(y - worldOffset, 0, worldSize - 1)) * worldSize + (math.clamp(x - worldOffset, 0, worldSize - 1));

                    grid[idxLocal] = action(grid[idxLocal], data[idx], modifier);
                }
            }
        }

        private static float PerfromMapAction_AddAnd (float currentValue, float mapValue, float modifier) => currentValue + mapValue * modifier;
        private static float PerfromMapAction_AddInversedAnd (float currentValue, float mapValue, float modifier) => currentValue + (1f - mapValue) * modifier;
        private static float PerfromMapAction_AddClamped (float currentValue, float mapValue, float modifier) => currentValue + math.max(mapValue, 0) * modifier;
        private static float PerfromMapAction_AddNegClamped (float currentValue, float mapValue, float modifier) => currentValue + math.min(mapValue, 0) * modifier;
        // private float PerfromMapAction_Multiply (float currentValue, float mapValue, float modifier) => currentValue * mapValue;
        // private float PerfromMapAction_MultiplyInversed (float currentValue, float mapValue, float modifier) => currentValue * (1f - mapValue);

        private delegate float PerfromMapAction (float currentValue, float mapValue, float modifier);


        // --------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetWorldPos (int2 position) => (new float2(position) + new float2(0.5f)) * InfluenceMapSystem.WORLD_SCALE;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetGridPos (float3 position) => new int2(math.floor(position.xz / InfluenceMapSystem.WORLD_SCALE));
        // --------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3X0Y (float2 position) => new Vector3(position.x, 0, position.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3MiddleX0Y (float2 position) => new Vector3(position.x, 0, position.y) + new Vector3(0.5f, 0, 0.5f) * InfluenceMapSystem.WORLD_SCALE;
        // --------------------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLocalIndex (int x, int y, int size) => y * size + x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetXY (int i, int size, int scale) => new int2(i & (size - 1), i >> scale);

        /*
               [MethodImpl(MethodImplOptions.AggressiveInlining)]
           private float2 GetWorldPos (int2 position) => new float2(position.x, position.y) * InfluenceMapSystem.WORLD_SCALE;
           [MethodImpl(MethodImplOptions.AggressiveInlining)]
           private int2 GetGridPos (float3 position) => new int2((int)math.floor(position.x / InfluenceMapSystem.WORLD_SCALE), (int)math.floor(position.z / InfluenceMapSystem.WORLD_SCALE));
           // --------------------------------------------------------
           [MethodImpl(MethodImplOptions.AggressiveInlining)]
           private Vector3 ToVector3X0Y (float2 position) => new Vector3(position.x, 0, position.y);
           [MethodImpl(MethodImplOptions.AggressiveInlining)]
           private Vector3 ToVector3MiddleX0Y (float2 position) => new Vector3(position.x, 0, position.y) + new Vector3(0.5f, 0, 0.5f) * InfluenceMapSystem.WORLD_SCALE;
           // --------------------------------------------------------
           [MethodImpl(MethodImplOptions.AggressiveInlining)]
           private int GetLocalIndex (int x, int y) => y * LocalSize + x;
           [MethodImpl(MethodImplOptions.AggressiveInlining)]
           private int2 GetXY (int i) => new int2(i & (LocalSize - 1), i >> LocalScale);
         */
    }
}