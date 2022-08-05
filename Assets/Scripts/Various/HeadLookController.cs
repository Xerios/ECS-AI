using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLookController : MonoBehaviour
{
    public float LookAtWeight;
    public Transform LookAtTarget;
    public Animator animator;

    void OnAnimatorIK (int layerIndex)
    {
        animator.SetLookAtWeight(LookAtWeight);
        if (LookAtWeight != 0f && LookAtTarget != null) animator.SetLookAtPosition(LookAtTarget.position + Vector3.up * 5f);
    }
}