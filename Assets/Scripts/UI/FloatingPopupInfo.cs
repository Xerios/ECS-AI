using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using Unity.Entities;
using UnityEngine;

public class FloatingPopupInfo : MonoBehaviour
{
    public JobInventoryData Data;
    public TextMeshProUGUI Counter, JobTitle;
    public Vector3 Position;
    private RectTransform rectTransform;

    private IDisposable disposable;
    private short JobId;
    private Entity SignalEntity;

    protected void Awake ()
    {
        rectTransform = (this.transform as RectTransform);
    }

    public void Set (Entity entity, short jobId)
    {
        JobId = jobId;
        SignalEntity = entity;
        JobTitle.text = Data.Jobs[JobId].Title;
        Refresh();
    }

    void OnEnable ()
    {
        disposable = Observable.Merge(GameManager.Instance.AgentJobChange, GameManager.Instance.Agents.ObserveCountChanged().AsUnitObservable()).Subscribe((_) => Refresh());
    }

    private void Refresh ()
    {
        var agents = GameManager.Instance.Agents;

        var min = 0;
        var value = agents.Count(x => x.JobId == JobId && x.AssignmentEntity == SignalEntity);
        var max = agents.Count(x => x.JobId == JobId && (x.AssignmentEntity == SignalEntity || x.AssignmentEntity == Entity.Null));

        Counter.text = $"{value}/{max}";
        if (value == min) {
            Counter.color = Color.gray;
        }else if (value == max) {
            Counter.color = new Color(0.2f, 1f, 0.2f);
        }else{
            Counter.color = Color.white;
        }
    }

    void OnDisable ()
    {
        if (disposable != null) disposable.Dispose();
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