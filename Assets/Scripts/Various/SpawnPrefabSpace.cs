using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabSpace : MonoBehaviour
{
    public GameObject prefab;
    public Bounds bounds;

    public float speed = 1f;
    public int count = 250;
    private int internalCounter = 0;
    private float nextTime;

    // Use this for initialization
    void Update ()
    {
        if (internalCounter >= count) return;

        if (nextTime < Time.time) {
            nextTime = Time.time + speed;

            float randX = Random.Range(bounds.min.x, bounds.max.x);
            float randY = Random.Range(bounds.min.y, bounds.max.y);
            float randZ = Random.Range(bounds.min.z, bounds.max.z);

            var go = Instantiate(prefab, this.transform.position + new Vector3(randX, randY, randZ), Quaternion.identity);
            if (go.GetComponent<Rigidbody>()) go.GetComponent<Rigidbody>().velocity = (Random.insideUnitSphere * 10f);
            internalCounter++;
        }
    }

    // Update is called once per frame
    void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.transform.position + bounds.center, bounds.size);

        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawCube(this.transform.position + bounds.center, bounds.size);
    }
}