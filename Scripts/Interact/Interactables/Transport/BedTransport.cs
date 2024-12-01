using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Interact.Interactables.Transport
{
    public class BedTransport : InteractableBase
    {
        [SerializeField] private string targetSceneName;
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private float sleepTransitionDelay = 1.5f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private Transform bedCameraPosition;
        [SerializeField] private float cameraTransitionSpeed = 2f;
        [SerializeField] private AudioClip sleepSound;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private GameObject transitionText;
        [SerializeField] private float textDisplayDuration = 7f; 

        private Camera mainCamera;
        private Vector3 originalCameraPosition;
        private Quaternion originalCameraRotation;

        private void Start()
        {
            mainCamera = Camera.main;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (transitionText != null)
            {
                transitionText.SetActive(false);
            }
        }

        public override void OnInteract(PSXFirstPersonController player)
        {
            base.OnInteract(player);

            player.enabled = false;

            originalCameraPosition = mainCamera.transform.position;
            originalCameraRotation = mainCamera.transform.rotation;

            StartCoroutine(BedTransitionSequence(player));
        }

        private IEnumerator BedTransitionSequence(PSXFirstPersonController player)
        {
            float elapsedTime = 0f;
            while (elapsedTime < sleepTransitionDelay)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / sleepTransitionDelay);

                mainCamera.transform.position = Vector3.Lerp(originalCameraPosition, bedCameraPosition.position, t);
                mainCamera.transform.rotation = Quaternion.Lerp(originalCameraRotation, bedCameraPosition.rotation, t);

                yield return null;
            }

            mainCamera.transform.position = bedCameraPosition.position;
            mainCamera.transform.rotation = bedCameraPosition.rotation;

            yield return StartCoroutine(FadeScreen(1f));

            if (sleepSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(sleepSound);
            }

            if (transitionText != null)
            {
                transitionText.SetActive(true);
            }

            yield return new WaitForSeconds(textDisplayDuration);

            if (transitionText != null)
            {
                transitionText.SetActive(false);
            }

            yield return StartCoroutine(FadeScreen(0f));
            SceneManager.LoadScene(targetSceneName);
        }

        private IEnumerator FadeScreen(float targetAlpha)
        {
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
