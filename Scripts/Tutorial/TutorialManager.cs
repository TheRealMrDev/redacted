using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private float textDisplayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Queue<string> tutorialQueue = new Queue<string>();
    private Coroutine displayCoroutine;
    private bool isDisplaying = false;

    private void Start()
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0;
        }
    }
    public void QueueTutorialMessage(string message)
    {
        tutorialQueue.Enqueue(message);

        if (!isDisplaying)
        {
            StartDisplayingMessages();
        }
    }

    private void StartDisplayingMessages()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(DisplayTutorialMessages());
    }

    private IEnumerator DisplayTutorialMessages()
    {
        isDisplaying = true;

        while (tutorialQueue.Count > 0)
        {
            string currentMessage = tutorialQueue.Dequeue();

            if (tutorialCanvasGroup.alpha > 0)
            {
                yield return StartCoroutine(FadeTutorialText(false));
            }
            tutorialText.text = currentMessage;
            yield return StartCoroutine(FadeTutorialText(true));
            yield return new WaitForSeconds(textDisplayDuration);
            yield return StartCoroutine(FadeTutorialText(false));
        }

        isDisplaying = false;
    }

    private IEnumerator FadeTutorialText(bool fadeIn)
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            
            tutorialCanvasGroup.alpha = fadeIn 
                ? Mathf.Lerp(0, 1, normalizedTime) 
                : Mathf.Lerp(1, 0, normalizedTime);

            yield return null;
        }
        tutorialCanvasGroup.alpha = fadeIn ? 1 : 0;
    }
}