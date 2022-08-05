using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Engine
{
    public class ParticipatantLayeredCollection : ParticipatantCollection
    {
        private bool[] layersUsed;
        private uint[] layers;

        public ParticipatantLayeredCollection(params uint[] layers) : base(layers.Length)
        {
            this.layers = layers;
            this.layersUsed = new bool[layers.Length];
        }

        public int GetIndex (Entity id, uint layer)
        {
            for (int i = 0; i < list.Length; i++) {
                if (list[i] == id && layers[i] == layer) return i;
            }
            return -1;
        }

        public int Add (Entity id, uint layer, int prefered = -1)
        {
            if (!CanUse(id)) return -1;
            if (IsUsing(id)) return GetIndex(id);

            if (prefered != -1 && list[prefered] == Entity.Null && layers[prefered] == layer) {
                list[prefered] = id;
            }else{
                prefered = GetIndex(Entity.Null, layer);
                list[prefered] = id;
            }
            count++;

            return prefered;
        }

        public bool CanUse (Entity id, uint layer)
        {
            if (IsUsing(id)) return true;
            if (Locked) return false;
            if (Count >= capacity) return false;
            if (GetIndex(Entity.Null, layer) != -1) return true;
            return false;
        }
    }

    public class ParticipatantCollectionQ : ParticipatantCollection
    {
        private List<(Entity Entity, float Value)> engaged;

        public ParticipatantCollectionQ(int capacity) : base(capacity)
        {
            engaged = new List<(Entity, float)>(capacity);
        }

        public void Engage (Entity entity, float score)
        {
            engaged.Add((entity, score));
            engaged.OrderBy(x => x.Value);
        }

        public bool IsFirstEngaged (Entity entity)
        {
            var index = engaged.FindIndex(x => x.Entity == entity);

            return (index == 0);
        }

        public void RemoveEngaged (Entity entity)
        {
            engaged.RemoveAll(x => x.Entity == entity);
        }
    }

    public class ParticipatantCollection
    {
        public bool Locked = false;

        protected Entity[] list;

        protected int capacity = 10;
        protected int count;

        public int Count { get => count; }
        public int Capacity { get => capacity; }

        public bool IsEmpty { get => Count == 0; }
        public bool IsFull { get => Count >= capacity; }

        public ParticipatantCollection(int capacity)
        {
            list = new Entity[capacity];
            this.capacity = capacity;
            count = 0;
        }

        public Entity this[int index] {
            get => list[index];
        }

        public int Add (Entity id, int prefered = -1)
        {
            if (!CanUse()) return -1;
            if (IsUsing(id)) return GetIndex(id);

            if (prefered >= 0 && prefered < capacity && list[prefered] == Entity.Null) {
                list[prefered] = id;
            }else{
                prefered = GetIndex(Entity.Null);
                list[prefered] = id;
            }
            count++;

            return prefered;
        }

        public void Remove (Entity id)
        {
            if (!IsUsing(id)) return;

            list[GetIndex(id)] = Entity.Null;
            count--;
        }

        public void Clear ()
        {
            for (int i = 0; i < list.Length; i++) list[i] = Entity.Null;
            count = 0;
        }

        public int GetIndex (Entity id) => Array.IndexOf(list, id);
        public bool IsIndexFree (int i) => list[i] == Entity.Null;

        public bool IsUsing (Entity id)
        {
            for (int i = capacity - 1; i >= 0; i--) {
                if (id.Equals(list[i])) return true;
            }
            return false;
        }

        protected bool CanUse () => (!Locked && Count < capacity);
        public bool CanUse (Entity id) => CanUse() || IsUsing(id);

        public IEnumerable<Entity> GetNonEmptyEntities ()
        {
            return list.Where(x => !x.Equals(Entity.Null));
        }
    }
}