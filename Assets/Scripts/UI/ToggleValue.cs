using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleValue : MonoBehaviour
{
    public TMPro.TextMeshProUGUI ValueText;
    private Toggle toggle;

    public void Start ()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChange);
        OnValueChange(toggle.isOn);
    }

    private void OnValueChange (bool value)
    {
        if (value) {
            ValueText.text = $"True";
        }else{
            ValueText.text = $"<color=#666>False</color>";
        }
    }

    public void OnDestroy ()
    {
        toggle?.onValueChanged.RemoveListener(OnValueChange);
    }
}