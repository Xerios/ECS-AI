using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Designation")]
public class JobInventoryData : ScriptableObject
{
    public ItemAssignment[] Items;
    public Designation[] Jobs;
    public JobItemData[] DefaultMindsets;

    public List<short> GetMatchinAbilityJobs (byte ability)
    {
        List<short> ids = new List<short>(3);

        for (short i = 0; i < Jobs.Length; i++) {
            // Debug.Log($"Match {ab} & {ability} == {((ab & ability))}");
            if (IsAbilityCompatible(i, ability)) {
                ids.Add(i);
            }
        }
        return ids;
    }

    public bool IsAbilityCompatible (short id, byte ability)
    {
        byte ab = GetJobAbilities(id);

        return (ab & ability) != 0;
    }


    public byte GetJobAbilities (short jobId)
    {
        byte abilities = (byte)0;

        // Get default mindset abilities
        foreach (var itemData in DefaultMindsets) {
            foreach (var dse in itemData.Mindset.DSEs) {
                foreach (var consid in dse.Considerations) {
                    if (consid.DataType == UtilityAI.ConsiderationMap.Types.HasAssignment) {
                        abilities |= consid.Value.Property;
                    }
                }
            }
        }

        // Get all item abilities
        foreach (var itemData in Items) {
            if (itemData.JobId != jobId) continue;

            foreach (var dse in itemData.Data.Mindset.DSEs) {
                foreach (var consid in dse.Considerations) {
                    if (consid.DataType == UtilityAI.ConsiderationMap.Types.HasAssignment) {
                        abilities |= consid.Value.Property;
                    }
                }
            }
        }

        return abilities;
    }
}

[Serializable]
public struct ItemAssignment
{
    public int JobId;
    // public int SlotId;
    public JobItemData Data;
}

[Serializable]
public struct Designation
{
    public string Title;
}