using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct HealthStateData : IComponentData
{
    [NonSerialized] public float health;
    [NonSerialized] public float maxHealth;
    // [NonSerialized] public int deathTick;
    [NonSerialized] public Entity killedBy;

    public void SetMaxHealth (float maxHealth)
    {
        this.maxHealth = maxHealth;
        health = maxHealth;
    }

    public void Heal (float amount)
    {
        if (health <= 0)
            return;

        health += amount;
        if (health > 0) {
            health = maxHealth;
        }
    }
    public void ApplyDamage (ref DamageEvent damageEvent)
    {
        if (health <= 0)
            return;

        health -= damageEvent.damage;
        if (health <= 0) {
            killedBy = damageEvent.instigator;
            // deathTick = tick;
            health = 0;
        }
    }
    public static void SetMax (EntityManager entMgr, Entity entity)
    {
        var max = entMgr.GetComponentData<HealthStateData>(entity);

        entMgr.SetComponentData(entity, new HealthStateData { health = max.maxHealth, maxHealth = max.maxHealth });
    }

    public static void SetMax (Entity entity) => SetMax(AIManager.Instance.mgr, entity);
}