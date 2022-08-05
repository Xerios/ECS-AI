using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Placable : MonoBehaviour
{
    public Material PreviewMaterial;
    public bool PreviewMode = true;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    // Start is called before the first frame update
    void Start ()
    {
        if (PreviewMode) {
            if (GetComponent<NavMeshObstacle>() != null) GetComponent<NavMeshObstacle>().enabled = false;
            if (GetComponent<TiledBuildEffect>() != null) GetComponent<TiledBuildEffect>().enabled = false;

            GetComponent<EntityMonoBehaviour>().enabled = false;

            // Disable collisions
            var cchildren = GetComponentsInChildren<Collider>();
            foreach (Collider collid in cchildren) collid.enabled = false;

            // Clone material
            PreviewMaterial = new Material(PreviewMaterial);

            // Apply material
            var children = GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in children) {
                originalMaterials[rend] = rend.materials;
            }

            foreach (Renderer rend in children) {
                var mats = new Material[rend.materials.Length];
                for (var j = 0; j < rend.materials.Length; j++) {
                    mats[j] = PreviewMaterial;
                }
                rend.materials = mats;
            }
        }
    }

    public void DisablePreviewMode ()
    {
        PreviewMode = false;

        if (GetComponent<NavMeshObstacle>() != null) GetComponent<NavMeshObstacle>().enabled = true;
        if (GetComponent<TiledBuildEffect>() != null) GetComponent<TiledBuildEffect>().enabled = true;

        GetComponent<EntityMonoBehaviour>().enabled = true;

        // Enable collisions
        var cchildren = GetComponentsInChildren<Collider>();
        foreach (Collider collid in cchildren) collid.enabled = true;

        // Restore material
        foreach (var pair in originalMaterials) {
            pair.Key.materials = pair.Value;
        }
    }
}