using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


namespace Interact.Interactables.Transport
{
    public class DoorTransport : InteractableBase
    {
        [SerializeField] string targetSceneName;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private GameObject cameraEquipmentObject;
        [SerializeField] private AudioClip transitionClip;
        [SerializeField] private AudioSource audioSource;

        public override void OnInteract(PSXFirstPersonController player)
        {
            if (!cameraEquipmentObject.activeInHierarchy)
            {
                NotificationManager.Instance.ShowCameraEquipmentNotification();
                return;
            }

            base.OnInteract(player);

            StartCoroutine(TransitionToScene());
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
