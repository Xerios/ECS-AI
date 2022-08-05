using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderValue : MonoBehaviour
{
    public TMPro.TextMeshProUGUI ValueText;
    private Slider slider;

    public void Start ()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChange);
        OnValueChange(slider.value);
    }

    private void OnValueChange (float value)
    {
        if (value == slider.minValue) {
            ValueText.text = $"<color=#666>{value} / {slider.maxValue}</color>";
        }else if (value == slider.maxValue) {
            ValueText.text = $"<color=#3f3>{value} / {slider.maxValue}</color>";
        }else{
            ValueText.text = $"{value} / {slider.maxValue}";
        }
    }

    public void OnDestroy ()
    {
        slider?.onValueChanged.RemoveListener(OnValueChange);
    }
}