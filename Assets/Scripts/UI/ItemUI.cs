using Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public int Id;
    public TMPro.TextMeshProUGUI LabelText;

    public JobItemData data;
    public JobItemData Data {
        get {
            return data;
        }
        set {
            LabelText.text = value.GetIcon();
            data = value;
            GetComponent<Image>().color = value.GetColor();
        }
    }

    public bool interactable {
        set {
            LabelText.color = value ? Color.white : new Color(1f, 1f, 1f, 0.2f);
            GetComponent<Button>().interactable = value;
            GetComponent<Draggable>().interactable = value;
        }
    }
}