using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

namespace Interact.Interactables.Transport
{
    public class CarTransport : InteractableBase
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private bool requireAllObjectivesComplete = true;
        [SerializeField] private AudioClip transitionClip;
        [SerializeField] private AudioSource audioSource;

        public override void OnInteract(PSXFirstPersonController player)
        {
            if (requireAllObjectivesComplete && !AreAllRequiredObjectivesCompleted())
            {
                NotificationManager.Instance.ShowObjectivesNotification();
                return;
            }

            base.OnInteract(player);

            StartCoroutine(TransitionToScene());
        }

        private bool AreAllRequiredObjectivesCompleted()
        {
            if (PhotoObjectivesManager.Instance == null)
            {
                Debug.LogError("PhotoObjectivesManager not found in the scene!");
                return false;
            }

            var levels = PhotoObjectivesManager.Instance.GetLevels();
            var currentLevel = levels[PhotoObjectivesManager.Instance.GetCurrentLevelIndex()];

            return currentLevel.objectives
                .Where(obj => obj.isPictureRequired)
                .All(obj => obj.isCompleted);
        }

        private IEnumerator TransitionToScene()
        {
            yield return StartCoroutine(FadeScreen(1f, true));

            SceneManager.LoadScene(targetSceneName);

            yield return StartCoroutine(FadeScreen(0f, false));
        }

        private IEnumerator FadeScreen(float targetAlpha, bool playAudio)
        {
            if (playAudio && transitionClip != null && audioSource != null)
            {
                audioSource.clip = transitionClip;
                audioSource.Play();
            }

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / fadeDuration;

                fadeCanvasGroup.alpha = Mathf.Lerp(fadeCanvasGroup.alpha, targetAlpha, normalizedTime);

                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
        }
    }
}
