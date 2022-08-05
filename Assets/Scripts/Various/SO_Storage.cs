using Engine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_Storage : EntityMonoBehaviour
{
    private ParticipatantCollection participants = new ParticipatantCollection(20);

    // Start is called before the first frame update
    public override void Init () => GameManager.Instance.StorageSpace = this.transform.position;
}