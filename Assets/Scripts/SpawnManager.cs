using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemy;
    public float delay = 20f;
    private float timeNext;

    // Start is called before the first frame update
    void Start ()
    {
        timeNext = (int)Time.time + 10f + delay;
        FindObjectOfType<TimelineUI>().SetEventMarker(timeNext);
    }

    // Update is called once per frame
    void Update ()
    {
        if (Time.time > timeNext) {
            timeNext = (int)Time.time + delay;
            FindObjectOfType<TimelineUI>().SetEventMarker(timeNext);
            Instantiate(enemy, new Vector3(60f, 0, 60f), Quaternion.identity);
        }
    }
}