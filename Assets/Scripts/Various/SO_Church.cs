using Engine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_Church : EntityMonoBehaviour
{
    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity();
        mgr.AddComponentData(entity, new SignalPosition { Value = transform.position });
        mgr.AddComponentData(entity, new SignalAbilityAssignment { Ability = (byte)UtilityAI.AbilityTags.Reclaim });
    }


    private void OnDrawGizmos ()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 1);
    }
}