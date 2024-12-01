using System.Collections;
using UnityEngine;

public class AmbienceAudio : MonoBehaviour
{
    [SerializeField] private AudioSource ambienceAudioSource;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float loweredVolume = 0.3f;
    [SerializeField] private float targetVolume = 0.15f;

    private void Start()
    {
        if (ambienceAudioSource != null)
        {
            ambienceAudioSource.loop = true;
            ambienceAudioSource.Play();
            
        }
        else
        {
            Debug.LogError("Ambience AudioSource is not assigned!");
        }
    }

    private void Update()
    {
        if (ambienceAudioSource != null && !ambienceAudioSource.isPlaying)
        {
            ambienceAudioSource.Play();
        }
    }

    private IEnumerator FadeAudio(AudioSource audioSource, bool fadeIn, float duration, float targetVolume = 0.15f)
    {
        if (audioSource == null) yield break;

        float startVolume = fadeIn ? 0f : audioSource.volume;
        float endVolume = fadeIn ? targetVolume : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, endVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = endVolume;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    private void OnEnable()
    {
        if (ambienceAudioSource != null)
        {
            ambienceAudioSource.Play();
            StartCoroutine(FadeAudio(ambienceAudioSource, true, fadeInDuration));
        }
    }
}