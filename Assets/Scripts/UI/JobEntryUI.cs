using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JobEntryUI : MonoBehaviour
{
    public short Id;
    public TMPro.TextMeshProUGUI label;
    public Button button;
    public Image background;
    public Color HightlightColor;

    public StepperUI stepper;
    public InventoryUI inventory;

    internal void Set (short id, Designation data, Action<short> onClick)
    {
        this.Id = id;
        label.text = data.Title;
        button.onClick.AddListener(() => onClick.Invoke(this.Id));
        // inventory.Data = data;
        inventory.Set(id, null);
    }

    public void Highlight ()
    {
        background.color = HightlightColor;
    }

    public void UnHighlight ()
    {
        background.color = Color.black;
    }
}