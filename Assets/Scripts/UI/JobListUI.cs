using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class JobListUI : MonoSingleton<JobListUI>
{
    // public int SelectedId = -1;
    public JobInventoryData Data;
    public Transform itemsContent;
    public GameObject itemPrefab;

    // public JobUI jobUI;

    private List<JobEntryUI> list = new List<JobEntryUI>(5);
    private IDisposable disposable;

    void Start ()
    {
        itemPrefab.SetActive(false);
    }

    void OnEnable ()
    {
        for (int i = 0; i < list.Count; i++) {
            Destroy(list[i].gameObject);
        }
        list.Clear();

        if (disposable != null) disposable.Dispose();

        for (int i = 0; i < Data.Jobs.Length; i++) {
            var data = Data.Jobs[i];
            var go = Instantiate(itemPrefab, itemsContent);
            var index = (short)i;
            var entry = go.GetComponent<JobEntryUI>();
            entry.Set(index, data, Select);
            go.SetActive(true);
            list.Add(entry);
        }
        disposable = Observable.Merge(GameManager.Instance.AgentJobChange, GameManager.Instance.Agents.ObserveCountChanged(true).AsUnitObservable()).Subscribe((_) => SetupSteppers());
    }

    void OnDestroy ()
    {
        disposable.Dispose();
    }

    void SetupSteppers ()
    {
        var agents = GameManager.Instance.Agents;

        for (int i = 0; i < list.Count; i++) {
            var entry = list[i];
            entry.stepper.Min = 0;
            entry.stepper.Max = agents.Count(x => x.JobId == entry.Id || x.JobId == -1);
            entry.stepper.Value = agents.Count(x => x.JobId == entry.Id);

            entry.stepper.OnChange = (last, newValue) => {
                if (last > newValue) {
                    var count = last - newValue;
                    for (int j = 0; j < agents.Count; j++) {
                        Engine.Agent agent = agents[j];
                        if (agent.JobId == entry.Id) {
                            ReclaimAndSetDefaultJob(agent);
                            count--;
                            if (count == 0) break;
                        }
                    }
                }else{
                    var count = newValue - last;
                    for (int j = 0; j < agents.Count; j++) {
                        Engine.Agent agent = agents[j];
                        if (agent.JobId == -1) {
                            DefineJob(entry, agent);
                            count--;
                            if (count == 0) break;
                        }
                    }
                }

                GameManager.Instance.AgentJobChange.Execute();

                SetupSteppers(); // Refresh all numbers
            };
        }
    }

    public void ReclaimAndSetDefaultJob (Engine.Agent agent)
    {
        agent.JobId = -1;
        agent.AssignmentEntity = Entity.Null;
        UtilityAI.Mind mind = agent.GetMind();
        mind.RemoveAllMindset();
        foreach (var item in Data.DefaultMindsets) {
            mind.AddMindset(item.Mindset);
        }
    }

    public void DefineJob (JobEntryUI entry, Engine.Agent agent)
    {
        agent.JobId = (short)entry.Id;
        agent.AssignmentEntity = Entity.Null;
        UtilityAI.Mind mind = agent.GetMind();
        foreach (var item in Data.DefaultMindsets) {
            mind.RemoveMindset(item.Mindset);
        }
        foreach (var item in Data.Items) {
            if (item.JobId == entry.Id) {
                mind.AddMindset(item.Data.Mindset);
            }
        }
    }

    // Update is called once per frame
    void Select (short id)
    {
        // this.gameObject.SetActive(false);
        // jobUI.SelectJob(id);
        // jobUI.gameObject.SetActive(true);
        FindObjectOfType<GameManager>().OnAssignClick(id);

        for (int i = 0; i < list.Count; i++) {
            var entry = list[i];
            if (entry.Id == id) {
                entry.Highlight();
            }else{
                entry.UnHighlight();
            }
        }
    }

    public void Deselect ()
    {
        for (int i = 0; i < list.Count; i++) {
            list[i].UnHighlight();
        }
    }

    private void RefreshAgentsList (Engine.Agent[] agents)
    {
        // for (int i = agentsContent.childCount - 1; i >= 1; i--) {
        //     Destroy(agentsContent.GetChild(i).gameObject);
        // }
        // for (int j = 0; j < agents.Length; j++) {
        //     var agent = agents[j];
        //     if (agent.JobId == SelectedId) {
        //         var go = Instantiate(agentPrefab, agentsContent);
        //         go.SetActive(true);
        //         go.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.Select(agent.gameObject));
        //         go.GetComponentInChildren<TextMeshProUGUI>().text = agent.name;
        //     }
        // }
    }
}