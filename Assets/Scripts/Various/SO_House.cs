using Engine;
using Game;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

[SelectionBase]
public class SO_House : EntityMonoBehaviour
{
    // public UniqueId id = UniqueId.Create("Building");
    private const int POPULATION_INCREASE = 2;

    public ParticipatantCollectionQ Participants = new ParticipatantCollectionQ(POPULATION_INCREASE);
    public int ParticipantsInside = 0;

    public AccessTags accessTags;
    public GameObject enableOnFull;

    // Start is called before the first frame update
    public override void Init ()
    {
        EntityManager mgr = AIManager.Instance.mgr;

        entity = mgr.CreateEntity();
        mgr.AddSharedComponentData(entity, new SignalGameObject { Value = this.gameObject });
        mgr.AddComponentData(entity, new SignalPosition { Value = transform.position });

        GameManager.Instance.Storage.Add(0, entity);

        GameResources.Max.Population += POPULATION_INCREASE;

        enableOnFull.SetActive(true);
    }

    public override void DeInit ()
    {
        GameManager.Instance.Storage.TryRemove(0, entity);

        GameResources.Max.Population -= POPULATION_INCREASE;
    }

    private void Update ()
    {
        if (entity.Equals(Entity.Null)) return;

        enableOnFull.SetActive(ParticipantsInside != 0);
    }


    private void OnDrawGizmos ()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;

        foreach (var ent in Participants.GetNonEmptyEntities()) {
            var pos = AIManager.Instance.mgr.GetComponentData<SignalPosition>(ent).Value;
            Gizmos.DrawLine(this.transform.position + Vector3.up, pos + Vector3.up);
        }

        Gizmos.color = Participants.IsEmpty ? Color.red : (Participants.IsFull && Time.time % 1f > 0.5f ? Color.blue : Color.green);
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 5);
    }
}