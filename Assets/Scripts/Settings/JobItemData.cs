using Engine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;

[CreateAssetMenu(menuName = "Job ItemData")]
public class JobItemData : ScriptableObject, IEquatable<JobItemData>
{
    public string Title;
    public string Description;
    public Mindset Mindset;

    public override bool Equals (object obj)
    {
        return Equals(obj as JobItemData);
    }

    public string GetIcon ()
    {
        return Title[0].ToString();
    }

    public Color GetColor ()
    {
        return ColorExtensions.GetColorFromString(Description.ToString());
    }

    public bool Equals (JobItemData other)
    {
        return other != null &&
               base.Equals(other) &&
               Title == other.Title &&
               Description == other.Description;
        //     &&
        //    IsUnique == other.IsUnique;
    }

    public override int GetHashCode ()
    {
        var hashCode = -731925194;

        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + System.Collections.Generic.EqualityComparer<string>.Default.GetHashCode (Title);
        hashCode = hashCode * -1521134295 + System.Collections.Generic.EqualityComparer<string>.Default.GetHashCode (Description);
        // hashCode = hashCode * -1521134295 + IsUnique.GetHashCode();
        return hashCode;
    }
}