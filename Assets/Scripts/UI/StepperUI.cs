using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class StepperUI : MonoBehaviour
{
    private int value = int.MinValue;

    private int min = 0;
    private int max = 10;
    public int step = 1;
    public TextMeshProUGUI Label;
    public Button zeroButton, minButton, plusButton, maxButton;

    public int Value { get => value; set => ChangeValue(value, false); }

    public bool Interactable { set => GetComponent<CanvasGroup>().interactable = value; }

    public int Min { get => min; set { min = value; ChangeValue(this.value, false); } }
    public int Max { get => max; set { max = value; ChangeValue(this.value, false); } }

    public Action<int, int> OnChange;

    // Start is called before the first frame update
    void Start ()
    {
        zeroButton.onClick.AddListener(() => ChangeValue(0));
        minButton.onClick.AddListener(() => ChangeValue(value - step));
        plusButton.onClick.AddListener(() => ChangeValue(value + step));
        maxButton.onClick.AddListener(() => ChangeValue(max));

        ChangeValue(value);
    }

    private void ChangeValue (int newValue, bool triggerEvent = true)
    {
        newValue = Mathf.Clamp(newValue, min, max);
        var oldValue = value;
        value = newValue;

        if (value == min) {
            Label.text = $"<color=#666>{value} / {max}</color>";
        }else if (value == max) {
            Label.text = $"<color=#3f3>{value} / {max}</color>";
        }else{
            Label.text = $"{value} / {max}";
        }

        zeroButton.interactable = (value != min);
        minButton.interactable = (value != min);
        plusButton.interactable = (value != max);
        maxButton.interactable = (value != max);

        if (triggerEvent && newValue != oldValue) OnChange?.Invoke(oldValue, value);
    }
}