using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseController : MonoBehaviour
{
    private bool Active = false;
    private CanvasGroup canvasGroup;

    // Use this for initialization
    void Start ()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public void Show (Vector3 pos)
    {
        Active = true;
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setIgnoreTimeScale(true);
        SetWorldPosition(pos);
    }

    public void Hide ()
    {
        Active = false;
        LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setIgnoreTimeScale(true);
    }

    public void SetWorldPosition (Vector3 pos)
    {
        if (canvasGroup.alpha == 0f) return;
        var canvasPos = Camera.main.WorldToScreenPoint(pos);
        canvasPos.z = 0;
        transform.position = canvasPos;
    }

    public void SetPosition (Vector3 screenPos)
    {
        if (!Active) return;
        transform.position = screenPos;
    }
}