using System.Collections;
using UnityEngine;

public class Spooka : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    private IEnumerator FadeAudio(bool fadeIn, float duration)
    {
        float startVolume = fadeIn ? 0f : audioSource.volume;
        float targetVolume = fadeIn ? 1f : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        if (!fadeIn) audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            audioSource.Play();
            StartCoroutine(FadeAudio(true, 1f));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeAudio(false, 1f));
        }
    }
}