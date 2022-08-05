using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UtilityAI
{
    [DisableAutoCreation]
    public class SpatialAwarnessSystem : JobComponentSystem
    {
        public static NativeMultiHashMap<int, EntityWithFactionPosition> gridDataHashMap;

        EntityQuery m_Group;

        protected override void OnCreateManager ()
        {
            m_Group = GetEntityQuery(
                    ComponentType.ReadOnly(typeof(SpatialTracking)),
                    ComponentType.ReadOnly(typeof(AgentFaction)),
                    ComponentType.ReadOnly(typeof(SignalPosition))
                    );

            gridDataHashMap = new NativeMultiHashMap<int, EntityWithFactionPosition>(0, Allocator.Persistent);
        }

        public const int gridCellSize = 40;

        private static int GetPositionHashMapKey (float3 position) => 32768 * (int)math.floor(position.x / gridCellSize) + (int)math.floor(position.z / gridCellSize);
        private static int Get2DPosHashMapKey (int2 position) => 32768 * position.x + position.y;


        [BurstCompile]
        [RequireComponentTag(typeof(SpatialTracking))]
        private struct SetEntityWithPositionHashMapJob : IJobForEachWithEntity<SignalPosition, AgentFaction>
        {
            [WriteOnly] public NativeMultiHashMap<int, EntityWithFactionPosition>.Concurrent nativeMultiHashMap;

            public void Execute (Entity entity, int index, [ReadOnly] ref SignalPosition translation, [ReadOnly] ref AgentFaction faction)
            {
                int hashMapKey = GetPositionHashMapKey(translation.Value);

                nativeMultiHashMap.Add(hashMapKey, new EntityWithFactionPosition { entity = entity, position = translation.Value.XZ(), faction = faction.LayerFlags });
            }
        }

        [BurstCompile]
        private struct TrackProximityEntitiesJob : IJobForEachWithEntity<SignalPosition, SpatialTrackProximity>
        {
            [ReadOnly] public NativeMultiHashMap<int, EntityWithFactionPosition> nativeMultiHashMap;
            [NativeDisableParallelForRestriction] public BufferFromEntity<EntityWithFactionPosition> buffer;

            public void Execute (Entity entity, int index, [ReadOnly] ref SignalPosition translation, [ReadOnly] ref SpatialTrackProximity spatialTrack)
            {
                float2 pos = translation.Value.XZ();

                float distSqr = spatialTrack.dist * spatialTrack.dist;

                var buff = buffer[entity];

                buff.Clear();

                float cellDist = spatialTrack.dist * 0.75f;
                int2 min = (int2)math.floor((pos - cellDist) / SpatialAwarnessSystem.gridCellSize);
                int2 max = (int2)math.ceil((pos + cellDist) / SpatialAwarnessSystem.gridCellSize);

                EntityWithFactionPosition entityPositionData;
                NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;

                for (int y = min.y; y < max.y; ++y) {
                    for (int x = min.x; x < max.x; ++x) {
                        // ---------------------------------
                        if (nativeMultiHashMap.TryGetFirstValue(Get2DPosHashMapKey(new int2(x, y)), out entityPositionData, out nativeMultiHashMapIterator)) {
                            int counter = 0;
                            do {
                                if (!entity.Equals(entityPositionData.entity) && math.distancesq(pos, entityPositionData.position) <= distSqr) {
                                    buff.Add(new EntityWithFactionPosition { entity = entityPositionData.entity, position = entityPositionData.position, faction = entityPositionData.faction });
                                    counter++;
                                    if (counter > EntityWithFactionPosition.CAPACITY_PER_CELL) return;
                                }
                            } while (nativeMultiHashMap.TryGetNextValue(out entityPositionData, ref nativeMultiHashMapIterator));
                        }
                    }
                }
            }
        }

        private static int GetEntityCountInHashMap (NativeMultiHashMap<int, EntityWithFactionPosition> gridDataHashMap, int quadrantHashMapKey)
        {
            EntityWithFactionPosition data;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            int count = 0;

            if (gridDataHashMap.TryGetFirstValue(quadrantHashMapKey, out data, out nativeMultiHashMapIterator)) {
                do {
                    count++;
                } while (gridDataHashMap.TryGetNextValue(out data, ref nativeMultiHashMapIterator));
            }
            return count;
        }


        protected override void OnDestroy ()
        {
            gridDataHashMap.Dispose();
        }

        protected override JobHandle OnUpdate (JobHandle handle)
        {
            gridDataHashMap.Clear();

            if (m_Group.CalculateLength() > gridDataHashMap.Capacity) {
                gridDataHashMap.Capacity = m_Group.CalculateLength();
            }

            // NativeHashMap<Entity, QuadrantData> entityTargetHashMap = new NativeHashMap<Entity, QuadrantData>(m_Group.CalculateLength(), Allocator.TempJob);

            // Position Units in HashMap
            var setQuadrantDataHashMapJob = new SetEntityWithPositionHashMapJob {
                nativeMultiHashMap = gridDataHashMap.ToConcurrent(),
            }.Schedule(m_Group, handle);
            setQuadrantDataHashMapJob.Complete();

            var trackProximity = new TrackProximityEntitiesJob {
                nativeMultiHashMap = gridDataHashMap,
                buffer = GetBufferFromEntity<EntityWithFactionPosition>(false)
            }.Schedule(this, handle);

            return trackProximity;
        }

        public static void DebugDrawQuadrant (float3 position, float radius)
        {
            // Vector3 lowerLeft = new Vector3(math.floor(position.x / gridCellSize) * gridCellSize, position.y, math.floor(position.z / gridCellSize) * gridCellSize);

            // Debug.DrawLine(lowerLeft, (Vector3)lowerLeft + new Vector3(quadrantCellSize, quadrantCellSize));
            // DrawQuad(lowerLeft, Color.green);
            // Debug.Log(GetPositionHashMapKey(position) + "    " + position);

            DrawEllipse(position, Vector3.up, Vector3.up, radius, radius, 64, Color.green, 0.3f);

            float2 p = new float2(position.x, position.z);
            int2 min = (int2)math.floor((p - radius) / SpatialAwarnessSystem.gridCellSize);
            int2 max = (int2)math.ceil((p + radius) / SpatialAwarnessSystem.gridCellSize);

            for (int y = min.y; y < max.y; ++y) {
                for (int x = min.x; x < max.x; ++x) {
                    int2 cell = new int2(x, y);
                    // int hash = cell.GetHashCode();

                    Vector3 pos = new Vector3(x * gridCellSize, position.y, y * gridCellSize);

                    DrawQuad(pos, Color.yellow);
                }
            }
            // int count = max.x - min.x;
        }


        // List<EntityWithFactionPosition> buffer = new List<EntityWithFactionPosition>(10);

        public void Tick ()
        {
            // RaycastManager.Instance.RaycastGround(Input.mousePosition, out Vector3 hitpos);

            // buffer.Clear();

            // float radius = 30f;
            // // DebugDrawQuadrant((float3)hitpos, radius);
            // GetDynamicsInRange(hitpos, radius, buffer, true);

            // foreach (var item in buffer) {
            //     Debug.DrawLine(hitpos + Vector3.up, item.position, Color.green, 0.3f);
            // }
        }

        public void GetDynamicsInRange (float2 pos, float dist, List<EntityWithFactionPosition> buffer, bool preview = false)
        {
            float distSqr = dist * dist;

            if (preview) DrawEllipse(new Vector3(pos.x, 0, pos.y), Vector3.up, Vector3.up, dist, dist, 64, Color.green, 0.3f);

            float cellDist = dist * 0.9f;
            int2 min = (int2)math.floor((pos - cellDist) / SpatialAwarnessSystem.gridCellSize);
            int2 max = (int2)math.ceil((pos + cellDist) / SpatialAwarnessSystem.gridCellSize);

            EntityWithFactionPosition entityPositionData;
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;

            for (int y = min.y; y < max.y; ++y) {
                for (int x = min.x; x < max.x; ++x) {
                    // if (preview) Debug.DrawRay(new Vector3(x * gridCellSize, 0, y * gridCellSize), Vector3.up * 5f, Color.magenta, 0.3f);
                    if (preview) DrawQuad(new Vector3(x * gridCellSize, 0, y * gridCellSize), Color.red);

                    if (gridDataHashMap.TryGetFirstValue(Get2DPosHashMapKey(new int2(x, y)), out entityPositionData, out nativeMultiHashMapIterator)) {
                        do {
                            if (math.distancesq(pos, entityPositionData.position) <= distSqr) {
                                buffer.Add(entityPositionData);
                            }
                        } while (gridDataHashMap.TryGetNextValue(out entityPositionData, ref nativeMultiHashMapIterator));
                    }
                }
            }
        }


        private static void DrawQuad (Vector3 lowerLeft, Color color)
        {
            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, 0, +0) * gridCellSize, color, 0.3f);
            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, 0, +1) * gridCellSize, color, 0.3f);
            Debug.DrawLine(lowerLeft + new Vector3(+1, 0, +0) * gridCellSize, lowerLeft + new Vector3(+1, 0, +1) * gridCellSize, color, 0.3f);
            Debug.DrawLine(lowerLeft + new Vector3(+0, 0, +1) * gridCellSize, lowerLeft + new Vector3(+1, 0, +1) * gridCellSize, color, 0.3f);
        }

        private static void DrawEllipse (Vector3 pos, Vector3 forward, Vector3 up, float radiusX, float radiusY, int segments, Color color, float duration = 0)
        {
            float angle = 0f;
            Quaternion rot = Quaternion.LookRotation(forward, up);
            Vector3 lastPoint = Vector3.zero;
            Vector3 thisPoint = Vector3.zero;

            for (int i = 0; i < segments + 1; i++) {
                thisPoint.x = Mathf.Sin(Mathf.Deg2Rad * angle) * radiusX;
                thisPoint.y = Mathf.Cos(Mathf.Deg2Rad * angle) * radiusY;

                if (i > 0) {
                    Debug.DrawLine(rot * lastPoint + pos, rot * thisPoint + pos, color, duration);
                }

                lastPoint = thisPoint;
                angle += 360f / segments;
            }
        }
    }
}