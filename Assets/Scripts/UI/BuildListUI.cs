using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BuildListUI : MonoBehaviour
{
    public BuildInventoryData Data;
    public Transform itemsContent;
    public GameObject itemPrefab;

    private List<BuildButton> list = new List<BuildButton>(5);

    void Start ()
    {
        itemPrefab.SetActive(false);
    }

    void OnEnable ()
    {
        for (int i = 0; i < list.Count; i++) {
            Destroy(list[i].gameObject);
        }
        list.Clear();

        for (int i = 0; i < Data.Items.Length; i++) {
            var data = Data.Items[i];
            var go = Instantiate(itemPrefab, itemsContent);
            var index = i;
            var entry = go.GetComponent<BuildButton>();
            entry.buildPrefab = data;
            go.SetActive(true);
            list.Add(entry);
        }
    }
}