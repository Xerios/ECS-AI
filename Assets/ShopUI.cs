using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public Transform itemsContent;
    public GameObject itemPrefab;
    public TMPro.TextMeshProUGUI cost;

    public GameObject[] ShopItems;

    private List<ShopItemUI> list = new List<ShopItemUI>(5);

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

        for (int i = 0; i < ShopItems.Length; i++) {
            var data = ShopItems[i];
            var go = Instantiate(itemPrefab, itemsContent);
            var index = (short)i;
            var entry = go.GetComponent<ShopItemUI>();
            entry.Set(data.name, 10);
            go.SetActive(true);
            list.Add(entry);
        }
    }
}