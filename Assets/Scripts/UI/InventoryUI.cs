using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public JobInventoryData Data;

    public Transform itemsContent;
    public GameObject itemPrefab;
    public bool IsReadOnly;
    [NonSerialized]
    public int ExcludeId = int.MinValue;

    private List<ItemUI> list = new List<ItemUI>(5);
    private int currentId;

    public void Set (int id, Action<int> onClick)
    {
        currentId = id;
        foreach (var item in list) Destroy(item.gameObject);
        list.Clear();

        for (int i = 0; i < Data.Items.Length; i++) {
            var data = Data.Items[i];
            if (data.JobId != id) continue;
            var go = Instantiate(itemPrefab, itemsContent);
            var index = i;
            ItemUI itemUI = go.GetComponent<ItemUI>();
            itemUI.Id = index;
            itemUI.Data = data.Data;

            if (!IsReadOnly) {
                Button button = go.GetComponent<Button>();
                button.onClick.AddListener(() => onClick?.Invoke(index));
            }else{
                Destroy(go.GetComponent<Draggable>());
            }
            list.Add(itemUI);
        }

        Refresh();
    }

    public void Refresh ()
    {
        if (IsReadOnly || ExcludeId == int.MinValue) return;
        // Debug.Log($"Refresh Current:{currentId}, Exclude:{ExcludeId}");

        for (int i = 0; i < list.Count; i++) {
            var entry = list[i];
            var contains = Data.Items.Any(x => x.JobId == ExcludeId && x.Data.GetInstanceID() == entry.Data.GetInstanceID());
            entry.interactable = !contains;
        }
    }

    internal void Add (ItemUI item)
    {
        // Debug.Log($"Add {item.Id} to {currentId}");
        list.Add(item);
        Data.Items[item.Id].JobId = currentId;
        item.transform.SetParent(itemsContent);
    }

    internal void Remove (ItemUI item)
    {
        // Debug.Log($"Remove {item.Id} from {currentId}");
        list.Remove(item);
        // Data.Items[item.Id].JobId = -1;
        // item.transform.SetParent(itemsContent);
    }
}