using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabSpaceOnce : MonoBehaviour
{
    public GameObject prefab;
    public Bounds bounds;

    public int amount = 1;

    // Use this for initialization
    void Start ()
    {
        for (int i = 0; i < amount; i++) {
            float randX = Random.Range(bounds.min.x, bounds.max.x);
            float randY = Random.Range(bounds.min.y, bounds.max.y);
            float randZ = Random.Range(bounds.min.z, bounds.max.z);

            Instantiate(prefab, new Vector3(randX, randY, randZ), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawCube(bounds.center, bounds.size);
    }
}