using Engine;
using Game;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

public class DamageOnContact : MonoBehaviour
{
    public string targetTag = "Unit";
    public int damage = 10;
    public GameObject spawnOnDestroy;

    [Header("Destroy object after :")]
    public float seconds = 10f;

    public void OnEnable ()
    {
        Destroy(this.gameObject, seconds);
    }

    void OnCollisionEnter (Collision collision)
    {
        bool collisionFound = false;
        Vector3 pos = Vector2.zero;
        Vector3 direction = Vector2.zero;

        foreach (ContactPoint contact in collision.contacts) {
            if (contact.otherCollider.tag == targetTag) {
                var target = contact.otherCollider.transform.root.gameObject.GetComponent<Agent>()?.GetEntity() ?? Unity.Entities.Entity.Null;
                if (target.Equals(Unity.Entities.Entity.Null)) continue;

                var damageEventBuffer = AIManager.Instance.mgr.GetBuffer<DamageEvent>(target);
                DamageEvent.AddEvent(damageEventBuffer, Entity.Null, damage, Vector3.zero, 0);

                // var ent = AIManager.Instance.mgr.CreateEntity(typeof(PropertyChangeMessage));
                // AIManager.Instance.mgr.SetComponentData(ent, new PropertyChangeMessage { Target = target, Type = (uint)PropertyType.Health, Value = -damage });

                collisionFound = true;
                direction = -contact.normal;
                pos = contact.point;
                break;
            }
        }
        if (!collisionFound) return;

        Rigidbody rb = this.transform.GetComponent<Rigidbody>();
        rb.detectCollisions = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;

        var duration = this.transform.GetComponentInChildren<ParticleSystem>().main.startLifetime.constant;
        GameObject.Instantiate(spawnOnDestroy, pos, Quaternion.LookRotation(direction, Vector3.forward));
        Destroy(this.gameObject, duration);
    }
}