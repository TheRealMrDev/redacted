using UnityEngine;

[System.Serializable]
public class ManifestSettings
{
    public float manifestationDuration = 1.0f;
    public AudioClip manifestationSound;
    public Material ghostMaterial;
}

public class PhotoManifest : MonoBehaviour
{
    [SerializeField] private ManifestSettings settings;
    [SerializeField] private float minViewDistance = 2f;
    [SerializeField] private float maxViewDistance = 10f;
    
    private bool isManifested = false;
    private bool isVisibleThroughCamera = false;
    private AudioSource audioSource;
    private Renderer[] objectRenderers;
    private Material[] originalMaterials;
    private Material[] ghostMaterials;
    private Camera cameraLens = null;
    private bool isPlayerZoomed = false;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        objectRenderers = GetComponentsInChildren<Renderer>();
        StoreAndSetupMaterials();
        SetVisibility(false);
    }

    private void StoreAndSetupMaterials()
    {
        if (objectRenderers.Length == 0)
        {
            Debug.LogError($"No renderers found on {gameObject.name}!");
            return;
        }

        originalMaterials = new Material[objectRenderers.Length];
        ghostMaterials = new Material[objectRenderers.Length];

        for (int i = 0; i < objectRenderers.Length; i++)
        {
            // Store original material
            originalMaterials[i] = objectRenderers[i].material;

            // Create ghost material
            if (settings.ghostMaterial != null)
            {
                ghostMaterials[i] = new Material(settings.ghostMaterial);
                if (originalMaterials[i].HasProperty("_MainTex"))
                {
                    ghostMaterials[i].SetTexture("_MainTex", originalMaterials[i].GetTexture("_MainTex"));
                }
            }
            else
            {
                ghostMaterials[i] = new Material(originalMaterials[i]);
                Color ghostColor = ghostMaterials[i].color;
                ghostColor.a = 0.5f;
                ghostMaterials[i].color = ghostColor;
            }
        }
    }

    private void SetVisibility(bool visible)
    {
        foreach (var renderer in objectRenderers)
        {
            renderer.enabled = visible;
        }
    }

    private void SetMaterials(Material[] materials)
    {
        for (int i = 0; i < objectRenderers.Length; i++)
        {
            objectRenderers[i].material = materials[i];
        }
    }

    private void Update()
    {
        if (isManifested) return;

        if (cameraLens == null || !isPlayerZoomed)
        {
            if (isVisibleThroughCamera)
            {
                SetVisibility(false);
                isVisibleThroughCamera = false;
            }
            return;
        }

        UpdateCameraVisibility();
    }

    private void UpdateCameraVisibility()
    {
        if (!isPlayerZoomed || cameraLens == null) return;

        float distance = Vector3.Distance(cameraLens.transform.position, transform.position);
        bool inRange = distance >= minViewDistance && distance <= maxViewDistance;

        Vector3 viewportPoint = cameraLens.WorldToViewportPoint(transform.position);
        bool inView = viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 
                     && viewportPoint.y >= 0 && viewportPoint.y <= 1;

        // Update visibility state
        if (inRange && inView && !isVisibleThroughCamera)
        {
            SetVisibility(true);
            SetMaterials(ghostMaterials);
            isVisibleThroughCamera = true;
        }
        else if ((!inRange || !inView) && isVisibleThroughCamera)
        {
            SetVisibility(false);
            isVisibleThroughCamera = false;
        }
    }

    public void OnCameraView(Camera camera)
    {
        cameraLens = camera;
        isPlayerZoomed = true;
    }

    public void OnCameraExit()
    {
        isPlayerZoomed = false;
        SetVisibility(false);
        isVisibleThroughCamera = false;
    }

    public bool TryManifest()
    {
        if (isManifested || !isVisibleThroughCamera) return false;

        StartCoroutine(ManifestObject());
        return true;
    }

    private System.Collections.IEnumerator ManifestObject()
    {
        isManifested = true;
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));

        if (settings.manifestationSound != null)
        {
            audioSource.PlayOneShot(settings.manifestationSound);
        }

        float elapsedTime = 0f;
        
        while (elapsedTime < settings.manifestationDuration)
        {
            float t = elapsedTime / settings.manifestationDuration;
            
            for (int i = 0; i < objectRenderers.Length; i++)
            {
                Material currentMaterial = objectRenderers[i].material;
                Color color = Color.Lerp(ghostMaterials[i].color, originalMaterials[i].color, t);
                currentMaterial.color = color;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetMaterials(originalMaterials);
        
        Destroy(this);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        
        obj.layer = newLayer;
        
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void OnDestroy()
    {
        if (ghostMaterials != null)
        {
            foreach (var material in ghostMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
    }
}