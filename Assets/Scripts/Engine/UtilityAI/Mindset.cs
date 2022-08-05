using System;
using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/New Mindset")]
    public class Mindset : ScriptableObject, ISerializationCallbackReceiver
    {
        private static short TotalId;

        public string Name;

        public Decision[] DSEs;


        void ISerializationCallbackReceiver.OnBeforeSerialize ()
        {}

        void ISerializationCallbackReceiver.OnAfterDeserialize ()
        {
            for (int i = 0; i < DSEs.Length; i++) {
                DSEs[i].Id = ++TotalId;
            }
        }

#if UNITY_EDITOR
        public static Mindset New (string name)
        {
            return new Mindset { Name = name, DSEs = new Decision[0] };
        }
#endif
    }
}