using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinToWorld : MonoBehaviour
{
    public Vector3 Position;
    private RectTransform rectTransform;

    void Awake ()
    {
        rectTransform = (this.transform as RectTransform);
    }

    void LateUpdate ()
    {
        Vector2 screenPos = Vector2.zero;

        screenPos = Camera.main.WorldToScreenPoint(Position);

        // var clampedScreenPos = new Vector3(screenPos.x, screenPos.y, 0);
        var clampedScreenPos = new Vector3(
                Mathf.Clamp(screenPos.x, rectTransform.sizeDelta.x, Screen.width - rectTransform.sizeDelta.x),
                Mathf.Clamp(screenPos.y, 0, Screen.height - rectTransform.sizeDelta.y - 100)
                , 0);


        rectTransform.position = clampedScreenPos;// Vector3.Lerp(rectTransform.position, clampedScreenPos, LeanTween.easeOutQuad(0f, 1f, positionTransition));
    }
}