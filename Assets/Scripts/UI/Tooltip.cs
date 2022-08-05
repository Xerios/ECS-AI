using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Tooltip : MonoSingleton<Tooltip>
{
    protected const float TRANSITION_TIME = 0.5f;
    protected const float TRANSITION_DELAY = 0.1f;

    public TMPro.TextMeshProUGUI IconText, LabelText, DescText;
    public RectTransform Content;
    public GameObject Arrow;

    private GameObject PinGameObject;
    private Vector3 PinPosition;
    private Vector2 Pin2DPosition;

    private CanvasGroup canvasGroup;
    private bool is2D = false;
    private bool isShowing = true;
    private float positionTransition;
    private RectTransform rectTransform;

    new void Awake ()
    {
        base.Awake();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = (this.transform as RectTransform);
        HideInstant();
    }

    void LateUpdate ()
    {
        Vector2 screenPos = Vector2.zero;

        screenPos = Pin2DPosition;
        positionTransition = Mathf.Clamp01(positionTransition + Time.unscaledDeltaTime);

        // var clampedScreenPos = new Vector3(screenPos.x, screenPos.y, 0);
        var clampedScreenPos = new Vector3(
                Mathf.Clamp(screenPos.x, 0, Screen.width - Content.sizeDelta.x),
                Mathf.Clamp(screenPos.y, 0, Screen.height - Content.sizeDelta.y - 100)
                , 0);

        // Arrow.SetActive(clampedScreenPos.x == screenPos.x && clampedScreenPos.y == screenPos.y);

        rectTransform.position = Vector3.Lerp(rectTransform.position, clampedScreenPos, LeanTween.easeOutQuad(0f, 1f, positionTransition));
    }

    public void Set (GameObject gameObject)
    {
        is2D = true;
        PinGameObject = gameObject;
        if (canvasGroup.alpha != 0) positionTransition = 0;

        var meta = PinGameObject.GetComponent<ItemUI>();
        if (meta != null) {
            IconText.text = meta.data.GetIcon();
            IconText.color = meta.data.GetColor();
            LabelText.text = meta.data.Title;
            DescText.text = meta.data.Description;
        }else{
            LabelText.text = "N/A";
            DescText.text = null;
        }
    }

    public void Show (GameObject pin)
    {
        if (PinGameObject != pin) {
            Set(pin);
        }
        Pin2DPosition = (Vector2)PinGameObject.transform.position + ((RectTransform)PinGameObject.transform).sizeDelta * new Vector2(1f, 0.5f);
        if (isShowing) return;
        isShowing = true;
        LeanTween.cancel(this.gameObject);
        this.gameObject.SetActive(true);
        LeanTween.alphaCanvas(canvasGroup, 1f, TRANSITION_TIME)
        .setIgnoreTimeScale(true)
        .setEase(LeanTweenType.easeOutExpo);
    }

    public void Hide ()
    {
        if (!isShowing) return;
        isShowing = false;
        LeanTween.alphaCanvas(canvasGroup, 0f, TRANSITION_TIME)
        .setIgnoreTimeScale(true)
        .setEase(LeanTweenType.easeOutCubic)
        .setDelay(TRANSITION_DELAY).setOnComplete(() => this.gameObject.SetActive(false));
    }

    public void HideInstant ()
    {
        if (!isShowing && canvasGroup.alpha == 0) return;
        isShowing = false;
        LeanTween.cancel(this.gameObject);
        canvasGroup.alpha = 0;
        positionTransition = 1f;
        this.gameObject.SetActive(false);
    }
}