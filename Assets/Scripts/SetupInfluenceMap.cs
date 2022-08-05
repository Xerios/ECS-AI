using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class SetupInfluenceMap : MonoBehaviour
{
    [Range(3, 7)]
    public int WorldScale = 6;
    public InfluenceMapSystem.InfluenceMapTypes Preview = InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES;

    private int WorldSize = 1;
    private int WorldOffset = -1;
    private Entity previewLayer = Entity.Null;

    // Start is called before the first frame update
    void Start ()
    {
        WorldSize = (int)math.pow(2, WorldScale);
        WorldOffset = -WorldSize >> 1;

        InfluenceMapSystem influenceMapSystem = Bootstrap.world.GetOrCreateSystem<InfluenceMapSystem>();

        influenceMapSystem.Init(WorldScale);

        // Setup layers
        EntityManager entityManager = Bootstrap.world.EntityManager;

        var unitsLayer = influenceMapSystem.Create(InfluenceMapSystem.InfluenceMapTypes.FACTION_0_UNITS, buffer: true, degrade: true, blur: true);
        entityManager.AddComponentData(unitsLayer, new InfluenceMap_AddUnits_Data { FactionLayerFlags = 1 << 0 });

        var unitsLayer2 = influenceMapSystem.Create(InfluenceMapSystem.InfluenceMapTypes.FACTION_1_UNITS, buffer: true, degrade: true, blur: true);
        entityManager.AddComponentData(unitsLayer2, new InfluenceMap_AddUnits_Data { FactionLayerFlags = 1 << 1 });

        var worldLayer = influenceMapSystem.Create(InfluenceMapSystem.InfluenceMapTypes.WORLD_OBSTACLES, clear: true, blur: true);
        entityManager.AddComponentData(worldLayer, new InfluenceMap_AddNavMesh_Data());
        entityManager.AddComponentData(worldLayer, new InfluenceMap_UpdateOnce());

        var fogLayer = influenceMapSystem.Create(InfluenceMapSystem.InfluenceMapTypes.FOG_OF_WAR_INTERNAL, buffer: true, normalize: true);
        entityManager.AddComponentData(fogLayer, new InfluenceMap_FilterUnits_Data { FactionLayerFlags = 1 << 0 });

        var fogLayerBlurred = influenceMapSystem.Create(InfluenceMapSystem.InfluenceMapTypes.FOG_OF_WAR, blur: true);
        entityManager.AddComponentData(fogLayerBlurred, new InfluenceMap_CopyFrom { Value = fogLayer });

        var buildSpace = influenceMapSystem.Create(InfluenceMapSystem.InfluenceMapTypes.BUILD_SPACE);
        entityManager.AddComponentData(buildSpace, new InfluenceMap_Circle { Radius = 10, Value = 1f });
        entityManager.AddComponentData(buildSpace, new InfluenceMap_UpdateOnce());

        // Setup preview
        previewLayer = influenceMapSystem.Get(Preview);
    }

    void OnValidate ()
    {
        WorldSize = (int)math.pow(2, WorldScale);
        WorldOffset = -WorldSize >> 1;

        if (!Application.isPlaying) return;

        previewLayer = Bootstrap.world.GetOrCreateSystem<InfluenceMapSystem>().Get(Preview);
    }

    private void OnDrawGizmosSelected ()
    {
        var scale = InfluenceMapSystem.WORLD_SCALE;

        // Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        // Gizmos.DrawCube(bounds.center, bounds.size);
        Gizmos.color = new Color(0f, 0f, 1f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, new float3(WorldSize, 1, WorldSize) * scale);

        if (!Application.isPlaying) return;
        if (previewLayer == Entity.Null) return;

        var data = Bootstrap.world.EntityManager.GetBuffer<InfluenceMapData>(previewLayer);

        for (int i = 0; i < data.Length; i++) {
            var posZ = i >> WorldScale;
            var posX = i & (WorldSize - 1);

            var pos = new float3(posX + WorldOffset + 0.5f, 0, posZ + WorldOffset + 0.5f) * scale;

            var influence = data[i].Value;
            Gizmos.color = influence < 0 ? new Color(1f, 0, 0, 1 - (1 + influence)) : new Color(0, 1f, 0, influence);
            if (influence == 0) continue;
            Gizmos.DrawCube(pos, new Vector3(scale, 0.2f, scale));
        }
    }
}