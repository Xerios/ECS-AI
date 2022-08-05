using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JobUI : MonoBehaviour
{
    public int SelectedId = -1;
    public JobInventoryData Data;

    public TMP_InputField titleInputText;
    public Button cancelButton, applyButton;
    public Button prevButton, nextButton;

    public DropZone dropZone, availableZone;

    public JobListUI jobWindow;
    public InventoryUI availableContent;
    public InventoryUI currentContent;

    private List<ItemUI> itemsAdded = new List<ItemUI>(3);
    private List<ItemUI> itemsRemoved = new List<ItemUI>(3);
    private string lastTitleInput;

    private bool editMode;
    public bool EditMode {
        set {
            cancelButton.interactable = value;
            applyButton.interactable = value;
            editMode = true;
        }
    }
    // Start is called before the first frame update
    void Start ()
    {
        dropZone.OnDropped = Added;
        dropZone.OnChangeOrder = ChangeOrder;
        dropZone.OnDragOut = Removed;
        prevButton.onClick.AddListener(() => SelectJob(SelectedId - 1));
        nextButton.onClick.AddListener(() => SelectJob(SelectedId + 1));

        titleInputText.onValueChanged.AddListener(OnChangeTitle);
        cancelButton.onClick.AddListener(OnCancel);
        applyButton.onClick.AddListener(OnApply);
        EditMode = false;
    }

    void OnEnable ()
    {
        EditMode = false;

        availableContent.Set(-1, OnItemClick);
    }

    private void OnChangeTitle (string newTitle)
    {
        EditMode = true;
    }

    private void OnItemClick (int itemId)
    {
        // Debug.Log($"Click: {itemId} => {Data.Items[itemId].JobId}");

        if (Data.Items[itemId].JobId == -1) {
            foreach (Transform item in availableContent.itemsContent) {
                if (item.GetComponent<ItemUI>().Id == itemId) {
                    item.SetParent(currentContent.itemsContent);
                    Added(item.gameObject);
                }
            }

            // Data.Items[itemId].JobId = SelectedId;
        }else{
            foreach (Transform item in currentContent.itemsContent) {
                if (item.GetComponent<ItemUI>().Id == itemId) {
                    item.SetParent(availableContent.itemsContent);
                    Removed(item.gameObject);
                }
            }

            // Data.Items[itemId].JobId = -1;
        }

        EditMode = true;
        availableContent.Refresh();
        currentContent.Refresh();
    }

    void OnDisable ()
    {
        OnCancel();
        jobWindow.gameObject.SetActive(true);
    }

    public void SelectJob (int id)
    {
        if (SelectedId != id) {
            OnCancel();
        }

        SelectedId = Mathf.Clamp(id, 0, Data.Jobs.Length - 1);

        lastTitleInput = Data.Jobs[SelectedId].Title;
        titleInputText.SetTextWithoutNotify(lastTitleInput);

        prevButton.interactable = SelectedId != 0;
        nextButton.interactable = SelectedId != Data.Jobs.Length - 1;

        currentContent.Set(SelectedId, OnItemClick);

        availableContent.ExcludeId = SelectedId;
        availableContent.Refresh();

        // Debug.Log(SelectedId + ": " + Data.GetJobAbilities(SelectedId));
    }


    private void Added (GameObject obj)
    {
        var itemUI = obj.GetComponent<ItemUI>();

        itemsAdded.Add(itemUI);
        availableContent.Remove(itemUI);
        currentContent.Add(itemUI);
        OnModify();
    }

    private void Removed (GameObject obj)
    {
        var itemUI = obj.GetComponent<ItemUI>();

        Data.Items[itemUI.Id].JobId = -1;
        itemsRemoved.Add(itemUI);
        currentContent.Remove(itemUI);
        availableContent.Add(itemUI);
        OnModify();
    }

    private void ChangeOrder (GameObject obj)
    {}

    private void OnModify ()
    {
        EditMode = true;
        currentContent.Refresh();
        availableContent.Refresh();
    }

    private void OnApply ()
    {
        var agents = GameManager.Instance.Agents;

        if (lastTitleInput != titleInputText.text) {
            Data.Jobs[SelectedId].Title = titleInputText.text;
            lastTitleInput = titleInputText.text;
        }

        foreach (var item in itemsAdded) {
            // Data.Items[item.Id].JobId = SelectedId;
            for (int j = 0; j < agents.Count; j++) {
                if (agents[j].JobId == SelectedId) agents[j].GetMind().AddMindset(Data.Items[item.Id].Data.Mindset);
            }
        }
        foreach (var item in itemsRemoved) {
            // Data.Items[item.Id].JobId = -1;
            for (int j = 0; j < agents.Count; j++) {
                if (agents[j].JobId == SelectedId) agents[j].GetMind().RemoveMindset(Data.Items[item.Id].Data.Mindset);
            }
        }

        itemsAdded.Clear();
        itemsRemoved.Clear();


        FloatingPopup.Instance.HideInstant();
        GameManager.Instance.AgentJobChange.Execute();

        EditMode = false;
        availableContent.Refresh();
    }

    private void OnCancel ()
    {
        if (!editMode) return;

        if (lastTitleInput != titleInputText.text) {
            titleInputText.SetTextWithoutNotify(lastTitleInput);
        }

        foreach (var item in itemsAdded) {
            Data.Items[item.Id].JobId = -1;
            currentContent.Remove(item);
            availableContent.Add(item);
        }
        foreach (var item in itemsRemoved) {
            Data.Items[item.Id].JobId = SelectedId;
            availableContent.Remove(item);
            currentContent.Add(item);
        }
        itemsAdded.Clear();
        itemsRemoved.Clear();
        EditMode = false;
        availableContent.Refresh();
    }
}