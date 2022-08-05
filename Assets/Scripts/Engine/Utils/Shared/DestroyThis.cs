using UnityEngine;
using System.Collections;

public class DestroyThis : MonoBehaviour {

    [Header("Destroy object after :")]
    public float seconds = 0f;

    public void OnEnable() {
        Destroy(this.gameObject, seconds);
    }
}
