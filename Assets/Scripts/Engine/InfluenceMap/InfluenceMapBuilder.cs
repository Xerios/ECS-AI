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
    public struct InfluenceBuilderMeta
    {
        public enum Types {
            FallOff,
            SetAllValues,
            ReduceValuesAround,
            Map_AddAnd,
            Map_AddInversedAnd,
            Map_AddClamped,
            Map_AddNegClamped,
        }
        public Types type;
        public byte map;
        public float2 pos;
        public float distance, weight;
    }

    public class InfluenceMapBuilder
    {
        private uint idHash;
        private bool generated;

        public bool IsGenerated { get => generated; }

        public int LocalScale;
        public int LocalSize;
        public int LocalOffset;

        public int2 gridPos, gridMin, gridMax;

        public NativeArray<float> grid;
        public Queue<InfluenceBuilderMeta> metas = new Queue<InfluenceBuilderMeta>();

        private float3 startPosition;
        private int size;
        internal Vector3 highestPosition, lowestPosition;

        public InfluenceMapBuilder(int size)
        {
            if (size > 4) Debug.LogError("Map too big for a local grid? Is this a mistake?");

            LocalScale = size;
            LocalSize = (int)math.pow(2, LocalScale);
            LocalOffset = LocalSize >> 1;
        }

        // --------------------------------------------------------

        public InfluenceMapBuilder Start (Vector3 position, uint id)
        {
            idHash = id;
            generated = false;
            metas.Clear();
            startPosition = position;
            gridPos = InfluenceMapSolver.GetGridPos(position);
            gridMin = gridPos - new int2(LocalOffset);
            gridMax = gridPos + new int2(LocalOffset);

            return this;
        }

        public void End () {}

        public void Solve ()
        {
            if (generated) return;

            grid = new NativeArray<float>(LocalSize * LocalSize, Allocator.Temp);
            InfluenceMapSolver.Solve(this);
            grid.Dispose();

            generated = true;
        }


        // ------------------------------------------

        public InfluenceMapBuilder SelfFalloff (float weight = 1)
        {
            metas.Enqueue(new InfluenceBuilderMeta {
                    type = InfluenceBuilderMeta.Types.FallOff,
                    pos = startPosition.xz,
                    distance = LocalOffset * LocalOffset,
                    weight = weight
                });
            return this;
        }

        public InfluenceMapBuilder AddFalloff (float3 pos, float distMax, float weight)
        {
            metas.Enqueue(new InfluenceBuilderMeta {
                    type = InfluenceBuilderMeta.Types.FallOff,
                    pos = pos.xz,
                    distance = distMax,
                    weight = weight
                });

            return this;
        }

        public InfluenceMapBuilder SetAllValues (float value)
        {
            metas.Enqueue(new InfluenceBuilderMeta {
                    type = InfluenceBuilderMeta.Types.SetAllValues,
                    weight = value
                });

            return this;
        }

        public InfluenceMapBuilder ReduceValuesAroundSelf (float distMax)
        {
            metas.Enqueue(new InfluenceBuilderMeta {
                    type = InfluenceBuilderMeta.Types.ReduceValuesAround,
                    pos = startPosition.xz,
                    distance = distMax
                });

            return this;
        }


        public InfluenceMapBuilder ActionWithMap (InfluenceBuilderMeta.Types type, byte mapType, float mod = 1f)
        {
            metas.Enqueue(new InfluenceBuilderMeta {
                    type = type,
                    map = mapType,
                    weight = mod,
                });

            return this;
        }

        public InfluenceMapBuilder AddMultipliedMap (byte mapType, float mod = 1f) => ActionWithMap(InfluenceBuilderMeta.Types.Map_AddAnd, mapType, mod);
        public InfluenceMapBuilder AddReversedMultipliedMap (byte mapType, float mod = 1f) => ActionWithMap(InfluenceBuilderMeta.Types.Map_AddInversedAnd, mapType, mod);
        public InfluenceMapBuilder AddPositiveClampedMap (byte mapType, float mod = 1f) => ActionWithMap(InfluenceBuilderMeta.Types.Map_AddClamped, mapType, mod);
        public InfluenceMapBuilder AddNegativeClampedMap (byte mapType, float mod = 1f) => ActionWithMap(InfluenceBuilderMeta.Types.Map_AddNegClamped, mapType, mod);

        // -------------------------------------


        public InfluenceMapBuilder DebugGizmos ()
        {
            var scale = InfluenceMapSystem.WORLD_SCALE;

            for (int i = 0; i < grid.Length; i++) {
                var pos = InfluenceMapSolver.GetWorldPos(gridMin + InfluenceMapSolver.GetXY(i, LocalSize, LocalScale)) - InfluenceMapSystem.WORLD_SCALE * 0.5f;
                var influence = grid[i];
                Gizmos.color = influence < 0 ? new Color(1f, 0, 0, 1 - (1 + influence)) : new Color(0, 1f, 0, influence);
                Gizmos.DrawCube(InfluenceMapSolver.ToVector3MiddleX0Y(pos), new Vector3(scale, 0.2f, scale));
            }

            Gizmos.color = new Color(1, 0, 0, 1f);
            Gizmos.DrawCube(lowestPosition, new Vector3(1f, 0.5f, 1f));

            Gizmos.color = new Color(0, 1, 0, 1f);
            Gizmos.DrawCube(highestPosition, new Vector3(1f, 0.5f, 1f));

            // Vector3 posRandom = ToWorldVector3X0Y(GetRandomHighestPosition());
            // Gizmos.color = new Color(1, 0, 1, 1f);
            // Gizmos.DrawCube(posRandom, new Vector3(1f, 0.5f, 1f));

            Gizmos.color = new Color(1, 1, 1, 1f);
            Gizmos.DrawWireCube(InfluenceMapSolver.ToVector3X0Y(InfluenceMapSolver.GetWorldPos(gridPos) - InfluenceMapSystem.WORLD_SCALE * 0.5f), new float3(LocalSize, 1, LocalSize) * scale);
            // Gizmos.color = new Color(0, 1, 0, 1f);
            // Gizmos.DrawWireCube(truePos, new float3(LocalSize, 1, LocalSize) * scale);
            return this;
        }
    }
}