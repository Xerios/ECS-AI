using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

namespace UtilityAI
{
    [Serializable]
    public class Decision : IEquatable<Decision>
    {
        public short Id { get; set; }

        public string Name;
        public float Weight = 1f;

        public uint Tags;

        public ConsiderationParams[] Considerations;

        public override string ToString () => $"{Name}";

        public bool Equals (Decision other)
        {
            return ReferenceEquals(other, null) ? false : Id == other.Id;
        }

#if UNITY_EDITOR
        public static Decision New (string name)
        {
            return new Decision
                   {
                       Name = name,
                       Considerations = new ConsiderationParams[0]
                   };
        }

        public void NewConsideration (IEnumerable<string> items)
        {
            foreach (var item in items) {
                ArrayUtility.Add<ConsiderationParams>(ref Considerations, ConsiderationParams.New(item));
            }
        }


        // private void CustomAddConsiderationButton ()
        // {
        //     // Finds all available stat-types and excludes the types that the statList already contains, so we don't get multiple entries of the same type.
        //     var availableStats = Enum.GetValues(typeof(ConsiderationMap.Types))
        //         .Cast<ConsiderationMap.Types>()
        //         .Except(Considerations.Select(x => x.DataType))
        //         .Select(x => x.ToString());

        //     // Here we then quickly make a popup selector, with no title, and support for multi-selection - because why not.
        //     var selector = new Sirenix.OdinInspector.Editor.GenericSelector<string>(null, true, availableStats);

        //     selector.SelectionTree.Config.DrawSearchToolbar = true;
        //     selector.SelectionConfirmed += NewConsideration;
        //     selector.ShowInPopup(Event.current.mousePosition + Vector2.left * 45, 200);
        // }
#endif
    }
}