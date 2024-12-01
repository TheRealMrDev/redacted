using UnityEngine;

public class RealisticLightFlicker : MonoBehaviour
{
    [Header("Flicker Intensity")]
    public float baseIntensity = 1f;
    public float intensityVariation = 0.2f;

    [Header("Flicker Frequency")]
    public float minFlickerInterval = 0.05f;
    public float maxFlickerInterval = 0.2f;

    [Range(0f, 1f)]
    public float flickerRandomness = 0.5f;
    public bool isCandle = false;
    public bool allowColorVariation = false;
    public float colorTemperatureVariation = 500f;

    private Light targetLight;
    private float nextFlickerTime;
    private float originalIntensity;
    private Color originalColor;

    void Start()
    {
        targetLight = GetComponent<Light>();
        if (targetLight == null)
        {
            Debug.LogError("RealisticLightFlicker requires a Light component!");
            enabled = false;
            return;
        }

        originalIntensity = targetLight.intensity;
        originalColor = targetLight.color;
        baseIntensity = originalIntensity;
    }

    void Update()
    {
        if (Time.time >= nextFlickerTime)
        {
            ApplyFlicker();
            nextFlickerTime = Time.time + Random.Range(minFlickerInterval, maxFlickerInterval);
        }
    }

    void ApplyFlicker()
    {
        float intensityMultiplier = GetIntensityMultiplier();
        targetLight.intensity = baseIntensity * intensityMultiplier;

        if (allowColorVariation)
        {
            ApplyColorVariation();
        }
    }

    float GetIntensityMultiplier()
    {
        if (isCandle)
        {
            // More organic, wave-like flicker for candle
            return 1f + Mathf.Sin(Time.time * 10f) * intensityVariation * 
                   Mathf.PerlinNoise(Time.time, 0f) * flickerRandomness;
        }
        else
        {
            return 1f + (Random.value - 0.5f) * intensityVariation * 2f * flickerRandomness;
        }
    }

    void ApplyColorVariation()
    {
        float temperatureShift = Random.Range(-colorTemperatureVariation, colorTemperatureVariation);
        targetLight.color = ColorTemperatureToRGB(originalColor, temperatureShift);
    }

    Color ColorTemperatureToRGB(Color baseColor, float temperatureOffset)
    {
        float t = baseColor.r + temperatureOffset / 10000f;
        float r = Mathf.Clamp01(t > 0.6f ? 1f : t > 0.3f ? 1f : t > 0.1f ? 0.8f : 0.5f);
        float g = Mathf.Clamp01(t > 0.6f ? 0.7f : t > 0.3f ? 0.9f : t > 0.1f ? 0.6f : 0.3f);
        float b = Mathf.Clamp01(t > 0.6f ? 0.4f : t > 0.3f ? 0.6f : t > 0.1f ? 0.4f : 0.2f);

        return new Color(r, g, b);
    }

    public void SimulateElectricalFailure(float duration)
    {
        StartCoroutine(ElectricalFailureRoutine(duration));
    }

    System.Collections.IEnumerator ElectricalFailureRoutine(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            targetLight.intensity = Mathf.Lerp(baseIntensity, 0f, Random.value);
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            elapsedTime += Time.deltaTime;
        }
        targetLight.intensity = baseIntensity;
    }
}