using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI label;
    public StepperUI stepper;

    internal void Set (string text, int maxCount)
    {
        label.text = text;
        stepper.Max = maxCount;
    }
}