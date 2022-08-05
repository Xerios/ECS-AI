using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class PreviewMapLayer : MonoBehaviour
{
    public InfluenceMapSystem.InfluenceMapTypes Preview = InfluenceMapSystem.InfluenceMapTypes.FOG_OF_WAR;
    private Texture2D texture;

    // Start is called before the first frame update
    void Start ()
    {
        InfluenceMapSystem influenceMapSystem = Bootstrap.world.GetOrCreateSystem<InfluenceMapSystem>();

        this.transform.localScale = Vector3.one * influenceMapSystem.WorldSize * InfluenceMapSystem.WORLD_SCALE;

        texture = new Texture2D(influenceMapSystem.WorldSize, influenceMapSystem.WorldSize);

        this.GetComponent<MeshRenderer>().material.SetTexture("_BaseMap", texture);
    }

    // Update is called once per frame
    void Update ()
    {
        var previewLayer = Bootstrap.world.GetOrCreateSystem<InfluenceMapSystem>().Get(Preview);
        var data = Bootstrap.world.EntityManager.GetBuffer<InfluenceMapData>(previewLayer).Reinterpret<float>();

        NativeArray<Color> colors = new NativeArray<Color>(data.Length, Allocator.TempJob);

        for (int i = 0; i < data.Length; i++) {
            var value = Mathf.Clamp01(1f - data[i]); // Mathf.Clamp01(data[i]);
            colors[i] = new Color(0, 0, 0, value);
        }
        texture.SetPixels(colors.ToArray());
        texture.Apply();
        colors.Dispose();
    }
}