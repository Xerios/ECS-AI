using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Narrative : MonoBehaviour
{
    public struct Narration
    {
        public string Text;
        public string[] Choices;
    }

    public List<Narration> List = new List<Narration>(3);

    public GameObject narration;
    public GameObject circleMask;
    public CanvasGroup canvas;
    public UnityEngine.UI.Button button;
    public TMPro.TextMeshProUGUI label;
    private bool FullScreenButtonPressed;

    private const float TRANSITION_SPEED = 5f;

    // Start is called before the first frame update
    void Start ()
    {
        button.onClick.AddListener(OnFullScreenClick);

        // List.Add(new Narration { Text = "Something has awoken you." });
        // List.Add(new Narration { Text = "A distant call, far in distant lands." });
        // List.Add(new Narration { Text = "You've slept for eons, almost forgotten, but still, \nthere's a faint call of your name, a cry for help." });

        // StartCoroutine(StartNarration());
    }

    private void OnFullScreenClick ()
    {
        FullScreenButtonPressed = true;
    }

    IEnumerator AwaitFullScreenClick ()
    {
        while (!FullScreenButtonPressed) yield return new WaitForSeconds(0.1f);
        FullScreenButtonPressed = false;
        yield return null;
    }

    IEnumerator  StartNarration ()
    {
        FindObjectOfType<TimelineUI>().ChangeTime(0);
        narration.SetActive(true);
        canvas.alpha = 0;
        while (List.Count != 0) {
            label.text = List[0].Text;
            yield return FadeIn();
            yield return AwaitFullScreenClick();
            List.RemoveAt(0);
            yield return FadeOut();
        }
        narration.SetActive(false);
        FindObjectOfType<TimelineUI>().ChangeTime(1);
        yield return null;
    }


    IEnumerator FadeOut ()
    {
        float alpha = canvas.alpha;

        LeanTween.cancel(circleMask);
        LeanTween.scale(circleMask, new Vector3(1, 1, 1) * 1.5f, 1f).setEaseOutExpo();

        while (alpha > 0) {
            alpha -= Time.unscaledDeltaTime * TRANSITION_SPEED;
            canvas.alpha = alpha;
            yield return null;
        }
    }

    IEnumerator FadeIn ()
    {
        float alpha = canvas.alpha;

        circleMask.transform.localScale = new Vector3(1, 1, 1) * 1.5f;
        LeanTween.cancel(circleMask);
        LeanTween.scale(circleMask, new Vector3(1, 1, 1), 1f).setEaseOutBack();

        while (alpha < 1) {
            alpha += Time.unscaledDeltaTime * TRANSITION_SPEED;
            canvas.alpha = alpha;
            yield return null;
        }
    }
}