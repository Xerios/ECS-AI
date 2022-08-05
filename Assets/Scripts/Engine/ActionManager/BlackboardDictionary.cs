using Engine;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace UtilityAI
{
    public class BlackboardDictionary
    {
        public BlackboardDictionary Parent;

        private Entity _self;
        public Entity Self {
            get {
                if (_self != Entity.Null) return _self;
                else if (Parent != null) return Parent.Self;
                return Entity.Null;
            }
            set{
                _self = value;
            }
        }

        private StringDictionary<object> dict = new StringDictionary<object>(3);
        private StringDictionary<Vector3> dictVector3 = new StringDictionary<Vector3>(3);
        private StringDictionary<float> dictFloat = new StringDictionary<float>(3);
        // private Dictionary<string, Entity > dictEntity = new Dictionary<string, Entity >(3);
        private Action<BlackboardDictionary> updateFunc;

        public BlackboardDictionary()
        {
            // ...
        }

        public BlackboardDictionary(BlackboardDictionary parent) : this()
        {
            Parent = parent;
        }

        // ---------------------------------------------------
        public void SetUpdate (Action<BlackboardDictionary> func)
        {
            updateFunc = func;
        }

        public void Update ()
        {
            Parent?.Update();
            updateFunc?.Invoke(this);
        }

        // ---------------------------------------------------

        public void Set (string name, object func) => dict[name] = func;
        public void SetBool (string name, bool func) => dict[name] = func;
        public void SetFloat (string name, float func) => dictFloat[name] = func;
        public void SetVec (string name, Vector3 func) => dictVector3[name] = func;
        // public void SetEntity (string name, Entity func) => dictEntity[name] = func;

        public bool Has (string name) => dict.ContainsKey(name);
        public bool HasBool (string name) => dict.ContainsKey(name);
        public bool HasFloat (string name) => dictFloat.ContainsKey(name);
        public bool HasVec (string name) => dictVector3.ContainsKey(name);
        // public bool HasEntity (string name) => dictEntity.ContainsKey(name);

        public bool Remove (string name) => dict.Remove(name);
        public bool RemoveBool (string name) => dict.Remove(name);
        public bool RemoveFloat (string name) => dictFloat.Remove(name);
        public bool RemoveVec (string name) => dictVector3.Remove(name);
        // public bool RemoveEntity (string name) => dictEntity.Remove(name);


        // ---------------------------------------------------
        // public Entity GetEntity (string name)
        // {
        //     Entity result;

        //     if (dictEntity.TryGetValue(name, out result)) return result;
        //     else if (Parent != null) return Parent.GetEntity(name);
        //     return default(Entity);
        // }

        public T Get<T>(string name)
        {
            return (T)Get(name);
        }

        public object Get (string name)
        {
            object result;

            if (dict.TryGetValue(name, out result)) return result;
            else if (Parent != null) return Parent.Get(name);
            return default(object);
        }

        public float GetFloat (string name)
        {
            float result;

            if (dictFloat.TryGetValue(name, out result)) return result;
            else if (Parent != null) return Parent.GetFloat(name);
            return default(float);
        }

        public Vector3 GetVec (string name)
        {
            Vector3 result;

            if (dictVector3.TryGetValue(name, out result)) return result;
            else if (Parent != null) return Parent.GetVec(name);
            return default(Vector3);
        }

        public bool GetBool (string name) => (bool)Get(name);
        public int GetInt (string name) => (int)Get(name);
    }
}