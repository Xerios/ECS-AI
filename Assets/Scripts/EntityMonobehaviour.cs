using Unity.Entities;
using UnityEngine;

[SelectionBase]
public class EntityMonoBehaviour : MonoBehaviour
{
    protected Entity entity;
    public bool IsDestroyed { get; private set; }
    private bool DoNotInitOnStart;

    public bool IsInit () => !entity.Equals(Entity.Null);

    public Entity GetEntity () => entity;

    public void NoAutoInit () => DoNotInitOnStart = true;

    // Use this for initialization
    void Start ()
    {
        if (!DoNotInitOnStart) StartInit();
    }

    public void StartInit ()
    {
        DoNotInitOnStart = true;
        AIManager.Instance.DeferredQueue.Enqueue(this.Init);
    }

    public virtual void Init ()
    {}

    public virtual void DeInit ()
    {}

    void OnApplicationQuit ()
    {
        IsDestroyed = true;
    }

    void OnDestroy ()
    {
        DestroyEntity();
    }

    public void DestroyEntity ()
    {
        if (IsDestroyed) return;
        if (entity.Equals(Entity.Null)) return;
        DeInit();
        if (AIManager.Instance.buffer.IsCreated) {
            AIManager.Instance.buffer.DestroyEntity(entity);
        }else{
            AIManager.Instance.mgr.DestroyEntity(entity);
        }
        entity = Entity.Null;
        IsDestroyed = true;
    }
}