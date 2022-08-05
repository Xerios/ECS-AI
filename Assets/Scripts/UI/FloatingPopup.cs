using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UniRx;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using UtilityAI;

[RequireComponent(typeof(CanvasGroup))]
public class FloatingPopup : MonoSingleton<FloatingPopup>
{
    public short SelectedId = -1;
    public JobInventoryData Data;

    protected const float TRANSITION_TIME = 0.5f;
    // protected const float TRANSITION_DELAY = 0.5f;

    public TextMeshProUGUI LabelText, DescText;
    public RectTransform Content;
    public GameObject Arrow, AssignSection;

    public TextMeshProUGUI titleText;
    public Button prevButton, nextButton;
    public StepperUI stepper;
    public Button cancelButton, applyButton;

    private GameObject PinGameObject;
    private Vector3 PinPosition;
    private Entity GetEntityFromGameObject () => PinGameObject.GetComponent<EntityMonoBehaviour>().GetEntity();

    private byte chosenAbility;
    private List<short> compatibleJobs;

    private CanvasGroup canvasGroup;
    private bool isShowing = true;
    private float positionTransition;
    private RectTransform rectTransform;
    private IDisposable disposable;

    public short LastIndex;

    public void Start ()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = (this.transform as RectTransform);

        prevButton.onClick.AddListener(() => SelectJob(SelectedId - 1));
        nextButton.onClick.AddListener(() => SelectJob(SelectedId + 1));

        applyButton.onClick.AddListener(Apply);
        cancelButton.onClick.AddListener(Cancel);

        HideInstant();
        stepper.OnChange = (last, newValue) => {
            applyButton.interactable = true;
        };
    }

    void OnEnable ()
    {
        if (AssignSection.activeSelf) {
            disposable = Observable.Merge(GameManager.Instance.AgentJobChange, GameManager.Instance.Agents.ObserveCountChanged(true).AsUnitObservable()).Subscribe((_) => Refresh());
        }
    }

    void OnDisable ()
    {
        if (disposable != null) disposable.Dispose();
    }

    void OnDestroy ()
    {
        if (disposable != null) disposable.Dispose();
    }

    private void Cancel ()
    {
        var PinEntity = GetEntityFromGameObject();

        if (!World.Active.EntityManager.HasComponent<SignalAssigned>(PinEntity)) throw new Exception("SignalAssigned doesn't exist on this entity");

        World.Active.EntityManager.RemoveComponent<SignalAssigned>(PinEntity);

        var jobId = compatibleJobs[SelectedId];

        var agents = GameManager.Instance.Agents;
        for (int j = 0; j < agents.Count; j++) {
            if (agents[j].JobId == jobId && agents[j].AssignmentEntity == PinEntity) {
                agents[j].AssignmentType = UtilityAI.AssignmentTypeData.NONE;
                agents[j].AssignmentEntity = Entity.Null;
            }
        }

        Select();  // Refresh UI

        if (World.Active.EntityManager.HasComponent<SignalDeleteOnCancel>(PinEntity)) {
            Destroy(PinGameObject);
            HideInstant();
        }
    }

    private void Apply ()
    {
        var PinEntity = GetEntityFromGameObject();

        var jobId = compatibleJobs[SelectedId];

        var agents = GameManager.Instance.Agents;
        var currentCount = agents.Count(x => x.JobId == jobId && x.AssignmentEntity == PinEntity);
        var newCount = stepper.Value;

        // Debug.Log($"Change {currentCount} > {newCount}");

        if (currentCount >= newCount) {
            var count = currentCount - newCount;
            for (int j = 0; j < agents.Count; j++) {
                if (agents[j].JobId == jobId && agents[j].AssignmentEntity == PinEntity) {
                    agents[j].AssignmentType = UtilityAI.AssignmentTypeData.NONE;
                    agents[j].AssignmentEntity = Entity.Null;
                    // Debug.Log($"Change {agents[j].GetEntity()} un-assigned");
                    count--;
                    if (count == 0) break;
                }
            }
        }else{
            var count = newCount - currentCount;
            for (int j = 0; j < agents.Count; j++) {
                if (agents[j].JobId == jobId && agents[j].AssignmentEntity == Entity.Null) {
                    agents[j].AssignmentType = chosenAbility;
                    agents[j].AssignmentEntity = PinEntity;
                    // Debug.Log($"Change {agents[j].GetEntity()} assigned");
                    count--;
                    if (count == 0) break;
                }
            }
        }

        if (!World.Active.EntityManager.HasComponent<SignalAssigned>(PinEntity)) {
            World.Active.EntityManager.AddComponentData(PinEntity, new SignalAssigned { JobId = jobId });
        }

        GameManager.Instance.AgentJobChange.Execute();

        Select(); // Refresh UI
    }

    private void Select ()
    {
        var PinEntity = GetEntityFromGameObject();

        if (World.Active.EntityManager.HasComponent<SignalAbilityAssignment>(PinEntity)) {
            chosenAbility = World.Active.EntityManager.GetComponentData<SignalAbilityAssignment>(PinEntity).Ability;
            compatibleJobs = Data.GetMatchinAbilityJobs(chosenAbility);
            // Debug.Log(compatibleJobs.Count);
        }

        if (World.Active.EntityManager.HasComponent<SignalAssigned>(PinEntity)) {
            var jobId = World.Active.EntityManager.GetComponentData<SignalAssigned>(PinEntity).JobId;
            var index = compatibleJobs.IndexOf(jobId);
            SelectJob(index);
        }else{
            SelectJob(LastIndex);
        }


        if (World.Active.EntityManager.HasComponent<SignalAssigned>(PinEntity)) {
            // Disable job id change
            prevButton.interactable = false;
            nextButton.interactable = false;

            // Enable cancel
            cancelButton.interactable = true;
            // Enable apply
            applyButton.interactable = false;
        }else{
            // Enable cancel
            cancelButton.interactable = false;
            // Enable apply
            applyButton.interactable = true;
        }
    }


    private void SelectJob (int id)
    {
        if (compatibleJobs == null || compatibleJobs.Count == 0) {
            AssignSection.SetActive(false);
            return;
        }

        SelectedId = (short)Mathf.Clamp(id, 0, compatibleJobs.Count - 1);
        LastIndex = SelectedId;

        var jobId = compatibleJobs[SelectedId];

        titleText.text = Data.Jobs[jobId].Title;
        prevButton.interactable = SelectedId != 0;
        nextButton.interactable = SelectedId != compatibleJobs.Count - 1;

        var agents = GameManager.Instance.Agents;

        // Fix all assigned & destroyed entities
        foreach (var agent in agents) {
            if (!World.Active.EntityManager.Exists(agent.AssignmentEntity)) {
                agent.AssignmentType = UtilityAI.AssignmentTypeData.NONE;
                agent.AssignmentEntity = Entity.Null;
            }
        }

        // Get count
        var PinEntity = GetEntityFromGameObject();

        Refresh();

        stepper.Value = agents.Count(x => x.JobId == jobId && x.AssignmentEntity == PinEntity);
    }


    private void Refresh ()
    {
        if (PinGameObject == null) return;

        SelectedId = (short)Mathf.Clamp(SelectedId, 0, compatibleJobs.Count - 1);

        var jobId = compatibleJobs[SelectedId];

        titleText.text = Data.Jobs[jobId].Title;
        prevButton.interactable = SelectedId != 0;
        nextButton.interactable = SelectedId != compatibleJobs.Count - 1;

        var agents = GameManager.Instance.Agents;
        var PinEntity = GetEntityFromGameObject();

        stepper.Min = 0;
        stepper.Max = agents.Count(x => x.JobId == jobId && (x.AssignmentEntity == PinEntity || x.AssignmentEntity == Entity.Null));
        stepper.Value = stepper.Value;
    }

    void LateUpdate ()
    {
        Vector2 screenPos = Vector2.zero;

        if (PinGameObject != null) {
            PinPosition = PinGameObject.transform.position + PinGameObject.GetComponentInChildren<MeshRenderer>().bounds.max._Y_() + Vector3.up;
            positionTransition = Mathf.Clamp01(positionTransition + Time.unscaledDeltaTime);
        }

        screenPos = Camera.main.WorldToScreenPoint(PinPosition);

        // var clampedScreenPos = new Vector3(screenPos.x, screenPos.y, 0);
        var clampedScreenPos = new Vector3(
                Mathf.Clamp(screenPos.x, Content.rect.width, Screen.width - Content.rect.width),
                Mathf.Clamp(screenPos.y, 0, Screen.height - Content.sizeDelta.y - 100)
                , 0);

        Arrow.SetActive(clampedScreenPos.x == screenPos.x && clampedScreenPos.y == screenPos.y);

        rectTransform.position = Vector3.Lerp(rectTransform.position, clampedScreenPos, LeanTween.easeOutQuad(0f, 1f, positionTransition));
    }

    public void Set (GameObject pin)
    {
        PinGameObject = pin;
        if (canvasGroup.alpha != 0) positionTransition = 0;

        var meta = PinGameObject.GetComponent<FloatingPopupMeta>();
        if (meta != null) {
            LabelText.text = meta.Label;
            DescText.text = MarkdownToTMPro.Convert(meta.Description);
        }else{
            LabelText.text = PinGameObject.name;
            DescText.text = "";
        }

        var PinEntity = GetEntityFromGameObject();
        if (World.Active.EntityManager.HasComponent<SignalAbilityAssignment>(PinEntity)) {
            // var abilities = World.Active.EntityManager.GetComponentData<SignalAbilityAssignment>(PinEntity).Ability;
            // if ((abilities & (byte)UtilityAI.AbilityTags.Reclaim) == (byte)UtilityAI.AbilityTags.Reclaim) DescText.text += "\n-Can be Reclaimed";
            // if ((abilities & (byte)UtilityAI.AbilityTags.Defend) == (byte)UtilityAI.AbilityTags.Defend) DescText.text += "\n-Can be Defended";
            // if ((abilities & (byte)UtilityAI.AbilityTags.Gather) == (byte)UtilityAI.AbilityTags.Gather) DescText.text += "\n-Can be Gathered";
            AssignSection.SetActive(true);
        }else{
            AssignSection.SetActive(false);
        }
    }

    public void Show (GameObject pin)
    {
        if (PinGameObject != pin) {
            Set(pin);
            Select();
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
        PinGameObject = null;
        isShowing = false;
        LeanTween.alphaCanvas(canvasGroup, 0f, TRANSITION_TIME)
        .setIgnoreTimeScale(true)
        .setEase(LeanTweenType.easeOutCubic)
        // .setDelay(TRANSITION_DELAY)
        .setOnComplete(() => this.gameObject.SetActive(false));
    }

    public void HideInstant ()
    {
        if (!isShowing && canvasGroup.alpha == 0) return;
        PinGameObject = null;
        isShowing = false;
        LeanTween.cancel(this.gameObject);
        canvasGroup.alpha = 0;
        positionTransition = 1f;
        this.gameObject.SetActive(false);
    }
}