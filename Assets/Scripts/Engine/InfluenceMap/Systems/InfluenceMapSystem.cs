using Engine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;
using UtilityAI;

[DisableAutoCreation]
[AlwaysUpdateSystem]
public partial class InfluenceMapSystem : JobComponentSystem
{
    private EntityQuery m_RaycastNecessaryGroup;

    public Dictionary<InfluenceMapTypes, Entity> mapDatas;
    // private EntityQuery m_Group;

    public int WorldScale;
    public int WorldSize;
    public int WorldOffset;
    public const int WORLD_SCALE = 4;

    public enum InfluenceMapTypes:byte {
        WORLD_OBSTACLES = 100,
        FOG_OF_WAR_INTERNAL = 101,
        FOG_OF_WAR = 102,
        BUILD_SPACE = 103,

        FACTION_0_UNITS = 0,
        FACTION_0_BUILDINGS = 1,
        FACTION_1_UNITS = 10,
        // FACTION_1_BUILDINGS =11,
    }

    public void Init (int mapSize)
    {
        mapDatas = new Dictionary<InfluenceMapTypes, Entity>(3);
        this.WorldScale = mapSize;
        this.WorldSize = (int)math.pow(2, mapSize);
        this.WorldOffset = -WorldSize >> 1;
    }

    protected override void OnCreateManager ()
    {
        m_RaycastNecessaryGroup = GetEntityQuery(
                typeof(InfluenceMapData),
                ComponentType.ReadOnly(typeof(InfluenceMapUpdate)),
                ComponentType.ReadOnly(typeof(InfluenceMap_AddNavMesh_Data)));
    }

    public Entity Create (InfluenceMapTypes mapType, bool buffer = false, bool clear = false, bool blur = false, bool degrade = false, bool normalize = false)
    {
        var entity = EntityManager.CreateEntity();

        var buffer2 = EntityManager.AddBuffer<InfluenceMapData>(entity);

        buffer2.ResizeUninitialized(WorldSize * WorldSize);
        for (int i = 0; i < buffer2.Capacity; i++) buffer2[i] = 0f;

        EntityManager.AddComponentData(entity, new InfluenceMapUpdate());

        if (buffer) EntityManager.AddBuffer<InfluenceMapToAddData>(entity);
        if (clear) EntityManager.AddComponentData(entity, new InfluenceMap_ClearData());
        if (blur) EntityManager.AddComponentData(entity, new InfluenceMap_BlurData());
        if (degrade) EntityManager.AddComponentData(entity, new InfluenceMap_DegradeData { Value = 0.6f });
        if (normalize) EntityManager.AddComponentData(entity, new InfluenceMap_NormalizeData());

        mapDatas.Add(mapType, entity);

        return entity;
    }

    public Entity Get (InfluenceMapTypes mapType)
    {
        Entity entity;

        if (mapDatas == null) return Entity.Null;

        if (!mapDatas.TryGetValue(mapType, out entity)) Debug.LogError($"InfluenceMap named \"{mapType}\"not found");
        return entity;
    }
    public Entity Get (byte mapType) => Get((InfluenceMapTypes)mapType);

    public void UpdateOnce (InfluenceMapTypes mapType)
    {
        var entity = Get(mapType);

        if (EntityManager.HasComponent<InfluenceMapUpdate>(entity)) return;

        EntityManager.AddComponentData(entity, new InfluenceMapUpdate());
    }

    struct InternalLayerPosUnit
    {
        public byte layer;
        public int2 pos;
    }


    [BurstCompile]
    struct GatherAgentsJob : IJobForEach<AgentFaction, SignalPosition>
    {
        public NativeList <InternalLayerPosUnit> result;
        public int WorldOffset;

        public void Execute ([ReadOnly] ref AgentFaction faction, [ReadOnly] ref SignalPosition position)
        {
            var pos = new int2((int)math.floor(position.Value.x / WORLD_SCALE), (int)math.floor(position.Value.z / WORLD_SCALE)) - WorldOffset;

            result.Add(new InternalLayerPosUnit { layer = faction.LayerFlags, pos = pos });
        }
    }


    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct ApplyAgentsInfluenceJob_Add : IJobForEachWithEntity_EC<InfluenceMap_AddUnits_Data>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapToAddData> bufferFromEnt;

        [ReadOnly] public NativeList <InternalLayerPosUnit> agents;

        public void Execute (Entity entity, int index, ref InfluenceMap_AddUnits_Data c0)
        {
            var buffer = bufferFromEnt[entity];

            buffer.Clear();
            for (int i = 0; i < agents.Length; i++) {
                var isMyFaction = ((c0.FactionLayerFlags & agents[i].layer) == agents[i].layer) ? 1f : -1f;

                InfluenceMapToAddData.AddData(buffer, agents[i].pos, isMyFaction, 1);
            }
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct ApplyAgentsInfluenceJob_Filter : IJobForEachWithEntity_EC<InfluenceMap_FilterUnits_Data>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapToAddData> bufferFromEnt;

        [ReadOnly] public NativeList <InternalLayerPosUnit> agents;

        public void Execute (Entity entity, int index, ref InfluenceMap_FilterUnits_Data c0)
        {
            var buffer = bufferFromEnt[entity];

            buffer.Clear();
            for (int i = 0; i < agents.Length; i++) {
                if ((c0.FactionLayerFlags & agents[i].layer) == agents[i].layer) InfluenceMapToAddData.AddData(buffer, agents[i].pos, 1f, 1);
            }
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct CopyMapDataJob : IJobForEachWithEntity_EC<InfluenceMap_CopyFrom>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        public void Execute (Entity entity, int index, [ReadOnly] ref InfluenceMap_CopyFrom c0)
        {
            bufferFromEnt[entity].CopyFrom(bufferFromEnt[c0.Value]);
        }
    }


    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct OverwriteValuesJob : IJobForEachWithEntity_EC<InfluenceMap_AddNavMesh_Data>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        [DeallocateOnJobCompletion][ReadOnly] public NativeArray <float> overwriteArray;

        public void Execute (Entity entity, int index, [ReadOnly] ref InfluenceMap_AddNavMesh_Data c0)
        {
            var buffer = bufferFromEnt[entity];

            for (int i = 0; i < overwriteArray.Length; i++) {
                if (overwriteArray[i] > 0) buffer[i] = 1f;
            }
        }
    }

    // [BurstCompile]
    // [RequireComponentTag(typeof(InfluenceMapUpdate))]
    // struct BuildRaycastsJob : IJobForEach_B<InfluenceMapData2>
    // {
    //     [ReadOnly] public NativeArray <RaycastCommand> commands;
    //     public int WorldSize, WorldScale, WorldOffset;
    //     public int LayerMask;

    //     public void Execute ([ReadOnly] DynamicBuffer<InfluenceMapData2> b0)
    //     {
    //         for (int i = 0; i < b0.Length; i++) {
    //             var pos2D = GetXY(i, WorldSize, WorldScale);
    //             var pos = new Vector3(pos2D.x + WorldOffset + 0.5f, 0.5f, pos2D.y + WorldOffset + 0.5f) * WORLD_SCALE;

    //             commands[i] = new RaycastCommand(pos + Vector3.up, Vector3.down, layerMask: LayerMask);
    //         }
    //     }
    // }

    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct DrawCircleJob : IJobForEachWithEntity_EC<InfluenceMap_Circle>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        public int WorldSize, WorldScale, WorldOffset;

        public void Execute (Entity entity, int index, [ReadOnly]  ref InfluenceMap_Circle c0)
        {
            var b0 = bufferFromEnt[entity];

            var pos = new int2(-WorldOffset, -WorldOffset);
            int size = WorldSize;
            float rSquared = size * size;

            var minX = 0;
            var minY = 0;
            var maxX = WorldSize;
            var maxY = WorldSize;

            for (int y = minY; y < maxY; y++) {
                for (int x = minX; x < maxX; x++) {
                    var trueIndex = y * WorldSize + x;
                    var val = (1f - math.distance(pos, new float2(x, y)) / size);
                    b0[trueIndex] += c0.Value * (val * val * val * val);
                }
            }
        }
    }

    [BurstCompile]
    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct DrawGradientCenteredJob : IJobForEachWithEntity_EC<InfluenceMap_Circle>
    {
        [NativeDisableParallelForRestriction] public BufferFromEntity <InfluenceMapData> bufferFromEnt;

        public int WorldSize, WorldScale, WorldOffset;

        public void Execute (Entity entity, int index, [ReadOnly]  ref InfluenceMap_Circle c0)
        {
            var b0 = bufferFromEnt[entity];

            var pos = new int2(-WorldOffset, -WorldOffset);
            int size = c0.Radius;
            float rSquared = size * size;

            var minX = math.max(0, pos.x - size);
            var minY = math.max(0, pos.y - size);
            var maxX = math.min(WorldSize, pos.x + size);
            var maxY = math.min(WorldSize, pos.y + size);


            for (int y = minY; y < maxY; y++) {
                for (int x = minX; x < maxX; x++) {
                    if (math.distancesq(pos, new float2(x, y)) < rSquared) {
                        var trueIndex = y * WorldSize + x;
                        b0[trueIndex] += c0.Value;
                    }
                }
            }
        }
    }


    [RequireComponentTag(typeof(InfluenceMapData), typeof(InfluenceMapUpdate))]
    struct UpdateOnceJob : IJobForEachWithEntity_EC<InfluenceMap_UpdateOnce>
    {
        [WriteOnly] public EntityCommandBuffer Cmd;

        public void Execute (Entity entity, int index, [ReadOnly]  ref InfluenceMap_UpdateOnce c0)
        {
            Cmd.RemoveComponent<InfluenceMapUpdate>(entity);
        }
    }

    protected override JobHandle OnUpdate (JobHandle handle)
    {
        JobHandle lastHandle = handle;

        var bufferFromEnt = GetBufferFromEntity<InfluenceMapData>(false);

        // Clear arrays
        Profiler.BeginSample("Clear Jobs");
        {
            lastHandle = new ClearMapDataJob { bufferFromEnt = bufferFromEnt }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // CopyFrom data
        Profiler.BeginSample("CopyFrom Jobs");
        {
            lastHandle = new CopyMapDataJob { bufferFromEnt = bufferFromEnt }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // Degrade data
        Profiler.BeginSample("DegradeMapData Jobs");
        {
            lastHandle = new DegradeMapDataJob { bufferFromEnt = bufferFromEnt }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // ------------------------------------------|
        // Find signals to add

        var agents = new NativeList <InternalLayerPosUnit>(200, Allocator.TempJob);

        Profiler.BeginSample("Gather Agents Pos & Layer");
        {
            new GatherAgentsJob { WorldOffset = WorldOffset, result = agents }.ScheduleSingle(this).Complete();
        }
        Profiler.EndSample();

        Profiler.BeginSample("Add Agents");
        {
            lastHandle = new ApplyAgentsInfluenceJob_Add {
                agents = agents,
                bufferFromEnt = GetBufferFromEntity<InfluenceMapToAddData>(false)
            }.Schedule(this, lastHandle);

            lastHandle = new ApplyAgentsInfluenceJob_Filter {
                agents = agents,
                bufferFromEnt = GetBufferFromEntity<InfluenceMapToAddData>(false)
            }.Schedule(this, lastHandle);
        }

        Profiler.EndSample();

        // // ------------------------------------------|

        if (m_RaycastNecessaryGroup.CalculateLength() != 0) {
            var defaultLayer = 1 << LayerMask.NameToLayer("Building");
            var collisionArray = new NativeArray<float>(WorldSize * WorldSize, Allocator.TempJob);

            for (int i = 0; i < WorldSize * WorldSize; i++) {
                var pos2D = GetXY(i, WorldSize, WorldScale);

                if (pos2D.x == 0 || pos2D.y == 0 || pos2D.x == WorldSize - 1 || pos2D.y == WorldSize - 1) {
                    collisionArray[i] = 1f;
                    continue;
                }
                var pos = new Vector3(pos2D.x + WorldOffset + 0.5f, 0.5f, pos2D.y + WorldOffset + 0.5f) * WORLD_SCALE;
                var hasObstacle = Physics.CheckSphere(pos, 2f, defaultLayer);
                // NavMeshHit hit;
                // var hasObstacle = NavMesh.Raycast(pos + Vector3.up, pos + Vector3.down, out hit, -1);
                // var hasObstacle = !NavMesh.SamplePosition(pos, out hit, 0.064f, defaultLayer);
                // Debug.DrawRay(pos, Vector3.up * 5, hasObstacle ? Color.red : Color.green, 10);
                // Debug.DrawRay(hit.position, hit.normal * 5, Color.blue, 10);

                if (hasObstacle) collisionArray[i] = 1f;
            }
            // // Using jobs
            // var results = new NativeArray<RaycastHit>(WorldSize * WorldSize, Allocator.Temp);
            // var commands = new NativeArray<RaycastCommand>(1, Allocator.Temp);

            // lastHandle = new BuildRaycastsJob {
            //     LayerMask = 1 << LayerMask.NameToLayer("Building"),
            //         WorldOffset = WorldOffset,
            //         WorldScale = WorldScale,
            //         WorldSize = WorldSize,
            //         commands = commands
            // }.Schedule(this, lastHandle);
            // // Schedule the batch of raycasts
            // JobHandle handlez = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));

            // // Wait for the batch processing job to complete
            // handle.Complete();

            // // Copy the result. If batchedHit.collider is null there was no hit
            // RaycastHit batchedHit = results[0];

            // // Dispose the buffers
            // results.Dispose();
            // commands.Dispose();


            // Make sure to complete our other jobs
            lastHandle.Complete();

            lastHandle = new OverwriteValuesJob {
                bufferFromEnt = bufferFromEnt,
                overwriteArray = collisionArray
            }.Schedule(this, lastHandle);
        }


        // // ------------------------------------------|

        // // Draw Circle
        Profiler.BeginSample("Draw Circle");
        {
            lastHandle = new DrawCircleJob {
                bufferFromEnt = bufferFromEnt,
                WorldOffset = WorldOffset,
                WorldScale = WorldScale,
                WorldSize = WorldSize,
            }.Schedule(this, lastHandle);

            lastHandle = new DrawGradientCenteredJob {
                bufferFromEnt = bufferFromEnt,
                WorldOffset = WorldOffset,
                WorldScale = WorldScale,
                WorldSize = WorldSize,
            }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // // ------------------------------------------|

        // Normalize
        Profiler.BeginSample("Normalize Jobs");
        {
            lastHandle = new NormalizeMapDataJob {
                bufferFromEnt = bufferFromEnt,
            }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // Add new things
        Profiler.BeginSample("Apply Influence Add Data Jobs");
        {
            lastHandle = new ApplyBufferedInfluenceJob {
                bufferFromEnt = bufferFromEnt,
                addvalues = GetBufferFromEntity<InfluenceMapToAddData>(true),
                WorldSize = WorldSize, WorldSizePowerOf2 = WorldSize * WorldSize
            }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // Blur data
        Profiler.BeginSample("BlurMapData Jobs");
        {
            lastHandle = new BoxBlurMapDataJob {
                bufferFromEnt = bufferFromEnt,
                WorldSize = WorldSize,
            }.Schedule(this, lastHandle);
        }
        Profiler.EndSample();

        // Used for update once

        EntityCommandBuffer cmds;
        Profiler.BeginSample("RemoveUpdate Jobs");
        {
            cmds = new EntityCommandBuffer(Allocator.TempJob);

            lastHandle = new UpdateOnceJob {
                Cmd = cmds
            }.ScheduleSingle(this, lastHandle);
        }
        Profiler.EndSample();

        lastHandle.Complete();

        cmds.Playback(EntityManager);
        cmds.Dispose();

        agents.Dispose();

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int2 GetXY (int i, int WorldSize, int WorldScale) => new int2(i & (WorldSize - 1), i >> WorldScale);


    protected override void OnDestroyManager ()
    {
        // Entities.ForEach((DynamicBuffer<InfluenceMapData2> data) => data.Data.Dispose());
    }
}