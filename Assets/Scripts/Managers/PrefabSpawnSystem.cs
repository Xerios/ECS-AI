using Engine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UtilityAI;

namespace Engine
{
    public struct SpawnSettings
    {
        public int Index;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
    }

    [UpdateBefore(typeof(AIManager))]
    [UpdateAfter(typeof(MindInitSystem))]
    public class PrefabSpawnSystem : ComponentSystem
    {
        private List<GameObject> objects;
        private NativeQueue<SpawnSettings> Queue;

        public void Add (GameObject gameObj, Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            var idx = objects.IndexOf(gameObj);

            if (idx == -1) {
                objects.Add(gameObj);
                idx = objects.Count - 1;
            }
            Queue.Enqueue(new SpawnSettings {
                    Index = idx,
                    Position = position,
                    Rotation = rotation,
                    Velocity = velocity
                });
        }

        protected override void OnCreateManager ()
        {
            objects = new List<GameObject>();
            Queue = new NativeQueue<SpawnSettings>(Allocator.Persistent);
        }


        protected override void OnDestroyManager ()
        {
            objects.Clear();
            if (Queue.IsCreated) Queue.Dispose();
        }

        protected override void OnUpdate ()
        {
            int i = 0;

            while (Queue.Count != 0 && i < 100) {
                var item = Queue.Dequeue();

                var go = GameObject.Instantiate(objects[item.Index], item.Position, item.Rotation);
                var rb = go.GetComponent<Rigidbody>();
                if (rb != null) rb.velocity = item.Velocity;
                i++;
            }
        }
    }
}