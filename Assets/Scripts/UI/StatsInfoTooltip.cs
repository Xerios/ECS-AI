using Engine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class StatsInfoTooltip : MonoSingleton<StatsInfoTooltip>
{
    protected const float TRANSITION_TIME = 0.5f;
    protected const float TRANSITION_DELAY = 0.5f;

    public JobInventoryData Data;
    public RectTransform Content;
    public TextMeshProUGUI LabelText, SubLabelText;
    public RectTransform ProgressBar;

    private GameObject PinGameObject;
    private Entity PinEntity;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private bool isShowing;

    public void Start ()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = (this.transform as RectTransform);

        HideInstant();
    }


    void LateUpdate ()
    {
        Vector2 screenPos = Vector2.zero;
        Vector3 PinPosition = Vector3.zero;

        if (PinGameObject != null) {
            PinPosition = PinGameObject.transform.position + PinGameObject.GetComponentInChildren<MeshRenderer>().bounds.max._Y_() + Vector3.up;

            if (World.Active.EntityManager.HasComponent<HealthStateData>(PinEntity)) {
                var hp = World.Active.EntityManager.GetComponentData<HealthStateData>(PinEntity);

                float value = hp.health / hp.maxHealth;
                ProgressBar.localScale = new Vector3(value, 1f, 1f);
            }else{
                ProgressBar.localScale = new Vector3(0f, 1f, 1f);
            }
        }

        screenPos = Camera.main.WorldToScreenPoint(PinPosition);

        var clampedScreenPos = new Vector3(
                Mathf.Clamp(screenPos.x, Content.rect.width, Screen.width - Content.rect.width),
                Mathf.Clamp(screenPos.y, 0, Screen.height - Content.sizeDelta.y - 100)
                , 0);

        rectTransform.position = clampedScreenPos;
    }


    public void Set (GameObject pin)
    {
        PinGameObject = pin;

        PinEntity = PinGameObject.GetComponent<EntityMonoBehaviour>().GetEntity();

        LabelText.text = PinGameObject.name;
        var id = PinGameObject.GetComponent<Agent>()?.JobId ?? -1;

        if (id == -1) {
            SubLabelText.text = null;
        }else{
            SubLabelText.text = Data.Jobs[id].Title;
        }
    }

    public void Show (GameObject pin)
    {
        if (PinGameObject != pin) {
            Set(pin);
        }
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
        .setDelay(TRANSITION_DELAY)
        .setOnComplete(() => this.gameObject.SetActive(false));
    }

    public void HideInstant ()
    {
        if (!isShowing && canvasGroup.alpha == 0) return;
        isShowing = false;
        LeanTween.cancel(this.gameObject);
        canvasGroup.alpha = 0;
        this.gameObject.SetActive(false);
    }
}