using Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class TimelineUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI LabelText;
    public RectTransform content;
    public RectTransform point, todayLine;
    public TMPro.TextMeshProUGUI pointText;

    public RawImage legendObject;
    public Button[] timeButtons;

    private float pointTime;

    const float deltaX = 120f;
    const float deltaXMin = deltaX * 0.05f;
    const float deltaXMax = deltaX * 0.95f;

    // Start is called before the first frame update
    void Start ()
    {
        var time = Time.time;
        var value = math.unlerp(time - deltaXMin, time + deltaXMax, time);

        todayLine.anchoredPosition = new Vector2(ValueToContentPosition(value), 0);

        timeButtons[1].interactable = false;
    }

    public void SetEventMarker (float time)
    {
        pointTime = time;
    }

    public void ChangeTime (int value)
    {
        timeButtons[0].interactable = true;
        timeButtons[1].interactable = true;
        timeButtons[2].interactable = true;
        timeButtons[3].interactable = true;

        if (value == 0) timeButtons[0].interactable = false;
        if (value == 1) timeButtons[1].interactable = false;
        if (value == 4) timeButtons[2].interactable = false;
        if (value == 10) timeButtons[3].interactable = false;

        Time.timeScale = value;
    }

    // Update is called once per frame
    void Update ()
    {
        var time = Time.time;

        var minTime = time - deltaXMin;
        var maxTime = time + deltaXMax;

        LabelText.text = "Time: " + new TimeSpan(0, 0, (int)time).FormatTimeSpan();

        var value = math.unlerp(minTime, maxTime, pointTime);

        point.anchoredPosition = new Vector2(ValueToContentPosition(value), 0);

        point.gameObject.SetActive(pointTime > minTime);

        pointText.text = (pointTime > time) ? (pointTime - time).ToString("F0") + "s" : null;

        legendObject.uvRect = new Rect(deltaXMin + (time + 5f) / deltaX * 12f, 0, 12f, 1f);
    }

    private float ValueToContentPosition (float value)
    {
        return -deltaXMin + (value * content.rect.width) - content.rect.width * 0.5f;
    }
}