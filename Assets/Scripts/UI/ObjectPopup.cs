using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ObjectPopup : MonoSingleton<ObjectPopup>
{
    public const float TRANSITION_TIME = 0.5f;

    public TMPro.TextMeshProUGUI titleText, descriptionText;

    public GameObject content, resourceElement, sliderElement, toggleElement;
    public Button destroyButton;

    private List<GameObject> elements;
    private bool isShowing = true;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private GameObject currentGameObject;

    public void Start ()
    {
        // Hide templates
        sliderElement.SetActive(false);
        toggleElement.SetActive(false);

        // Init
        elements = new List<GameObject>(3);
        rectTransform = (this.transform as RectTransform);
        canvasGroup = GetComponent<CanvasGroup>();
        HideInstant();

        destroyButton.onClick.AddListener(() => {
                if (!isShowing) return;
                FindObjectOfType<GameManager>().Deselect();
                FloatingPopup.Instance.HideInstant();
                Destroy(currentGameObject);
                HideInstant();
            });
    }

    // Start is called before the first frame update
    public bool Show (GameObject go)
    {
        var meta = go.GetComponent<FloatingPopupMeta>();

        if (meta == null) {
            Hide();
            return false;
        }

        currentGameObject = go;

        foreach (var item in elements) Destroy(item);

        // ----------------
        titleText.text = meta.Label;
        descriptionText.text = MarkdownToTMPro.Convert(meta.Description);

        // ----------------
        destroyButton.gameObject.SetActive(go.GetComponent<Placable>() != null);
        // ----------------

        var pool = go.GetComponent<ResourcePool>();

        resourceElement.SetActive(pool != null);

        if (pool != null) {
            resourceElement.GetComponentInChildren<TMPro.TextMeshProUGUI>().text =
                $"Wood:<indent=55%>{pool.Pool.Wood}</indent>\n" +
                $"Iron:<indent=55%>{pool.Pool.Iron}</indent>\n";

            AddSlider("Warriors", 1, 10, (value) => Debug.Log(value));
        }
        // AddSlider("Warriors", 1, 10);
        // AddSlider("Engineers", 6, 10);
        // AddSlider("Common Folk", 2, 10);

        // ----------------
        if (isShowing) return true;

        isShowing = true;
        LeanTween.cancel(this.gameObject);
        this.gameObject.SetActive(true);

        // LeanTween.scale(this.gameObject, Vector3.one * 1f, TRANSITION_TIME).setEase(LeanTweenType.easeInCirc);

        this.gameObject.SetActive(true);

        LeanTween.value(this.gameObject, rectTransform.anchoredPosition.x, 0f, TRANSITION_TIME)
        .setOnUpdate((value) => rectTransform.anchoredPosition = new Vector2(value, rectTransform.anchoredPosition.y))
        .setEase(LeanTweenType.easeOutExpo).setOnComplete(() => canvasGroup.interactable = true);

        return true;
    }

    private void AddSlider (string title, int value, int max, UnityAction<float> onChange)
    {
        var element = Instantiate(sliderElement, content.transform);

        element.transform.Find("Label").GetComponent<TMPro.TextMeshProUGUI>().text = title;
        var slider = element.transform.Find("Slider").GetComponent<UnityEngine.UI.Slider>();
        slider.value = value;
        slider.maxValue = max;
        slider.onValueChanged.AddListener(onChange);
        element.SetActive(true);

        elements.Add(element);
    }

    public void Hide ()
    {
        if (!isShowing) return;
        isShowing = false;
        canvasGroup.interactable = false;

        currentGameObject = null;
        // LeanTween.scale(this.gameObject, Vector3.one * 0.75f, TRANSITION_TIME).setEase(LeanTweenType.easeOutCirc);

        LeanTween.value(this.gameObject, rectTransform.anchoredPosition.x, Mathf.Abs(rectTransform.sizeDelta.x), TRANSITION_TIME)
        .setOnUpdate((value) => rectTransform.anchoredPosition = new Vector2(value, rectTransform.anchoredPosition.y))
        .setEase(LeanTweenType.easeOutQuint)
        .setOnComplete(() => this.gameObject.SetActive(false));
    }

    public void HideInstant ()
    {
        if (!isShowing) return;
        LeanTween.cancel(this.gameObject);
        isShowing = false;
        currentGameObject = null;
        canvasGroup.interactable = false;
        rectTransform.anchoredPosition = new Vector2(Mathf.Abs(rectTransform.sizeDelta.x), rectTransform.anchoredPosition.y);
        this.gameObject.SetActive(false);
    }
}