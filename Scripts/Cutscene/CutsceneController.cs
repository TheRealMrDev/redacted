using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    [Header("Cutscene Components")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject phoneObject;

    [Header("Camera Movement")]
    [SerializeField] private float cameraMoveDuration = 3f;
    [SerializeField] private Vector3 cameraEndPosition;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private void Start()
    {
        // Start the cutscene when the script begins
        StartCutscene();
    }

    private void StartCutscene()
    {
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        // Fade in from black
        yield return StartCoroutine(FadeCanvasGroup(fadeCanvasGroup, 1f, 0f, fadeDuration));

        // Start audio
        audioSource.Play();

        // Move camera towards phone
        yield return StartCoroutine(MoveCameraToTarget());

        // Wait for audio to finish
        yield return new WaitForSeconds(audioSource.clip.length - cameraMoveDuration);

        // Fade to black
        yield return StartCoroutine(FadeCanvasGroup(fadeCanvasGroup, 0f, 1f, fadeDuration));

        // Change scene (replace with your scene transition method)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Test");
    }

    private IEnumerator MoveCameraToTarget()
    {
        Vector3 startPosition = mainCamera.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < cameraMoveDuration)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / cameraMoveDuration;

            // Smooth interpolation using SmoothStep for more natural movement
            mainCamera.transform.position = Vector3.Lerp(startPosition, cameraEndPosition, Mathf.SmoothStep(0, 1, percentageComplete));

            yield return null;
        }

        // Ensure camera ends exactly at the target position
        mainCamera.transform.position = cameraEndPosition;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / duration;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, percentageComplete);

            yield return null;
        }

        // Ensure final alpha is set precisely
        canvasGroup.alpha = endAlpha;
    }
}