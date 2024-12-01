using UnityEngine;

public class PhotoOnBoard : MonoBehaviour
{
    public PhotoMetadata Metadata { get; private set; }
    public string PhotoPath { get; private set; }
    
    [SerializeField] private Material defaultSecretMaterial;
    
    // Add a field to store the secret material path
    public string SecretMaterialPath { get; private set; }
    
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            if (GetComponent<MeshFilter>() == null)
            {
                MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                meshFilter.mesh = CreatePhotoMesh();
            }
        }
    }
    
    public void Initialize(Material material, PhotoMetadata metadata, string path)
    {
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer is missing! Adding one...");
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    
        meshRenderer.material = material;
        Metadata = metadata;
        PhotoPath = path;

        if (!TryGetComponent<BoxCollider>(out var photoCollider))
        {
            photoCollider = gameObject.AddComponent<BoxCollider>();
        }
        photoCollider.size = new Vector3(1.1f, 1.1f, 0.1f);
    
        // Apply secret material
        ApplySecretMaterial();
    }

    private void ApplySecretMaterial()
    {
        Transform secretChild = transform.Find("Secret");
        if (secretChild != null)
        {
            Material secretMaterial = null;

            // First, try to get from PhotoPersistenceManager
            if (!string.IsNullOrEmpty(PhotoPath))
            {
                string persistedMaterialPath = PhotoPersistenceManager.Instance
                    ?.GetPhotoSecretMaterialPath(PhotoPath);

                if (!string.IsNullOrEmpty(persistedMaterialPath))
                {
                    secretMaterial = Resources.Load<Material>(persistedMaterialPath);
                }
            }

            if (secretMaterial == null)
            {
                if (!string.IsNullOrEmpty(SecretMaterialPath))
                {
                    secretMaterial = Resources.Load<Material>(SecretMaterialPath);
                }

                if (secretMaterial == null)
                {
                    secretMaterial = GetSecretMaterialFromObjective();
                }
            }

            MeshRenderer secretRenderer = secretChild.GetComponent<MeshRenderer>();
            if (secretRenderer != null)
            {
                secretRenderer.material = secretMaterial ?? defaultSecretMaterial;
            }
        }
    }

    private Material GetSecretMaterialFromObjective()
    {
        if (PhotoObjectivesManager.Instance != null)
        {
            int currentLevelIndex = PhotoObjectivesManager.Instance.GetCurrentLevelIndex();
            var levels = PhotoObjectivesManager.Instance.GetLevels();
            
            if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
            {
                var currentLevel = levels[currentLevelIndex];
                foreach (var objective in currentLevel.objectives)
                {
                    if (objective.isCompleted && 
                        objective.targetObject != null && 
                        objective.secretMaterial != null)
                    {
                        
                        if (!string.IsNullOrEmpty(PhotoPath))
                        {
                            PhotoPersistenceManager.Instance?.SavePhotoSecretMaterial(
                                PhotoPath, SecretMaterialPath);
                        }
                        
                        return objective.secretMaterial;
                    }
                }
            }
        }
        return null;
    }
    

    public void SetSecretMaterial(Material material)
    {
        Transform secretChild = transform.Find("Secret");
        if (secretChild != null)
        {
            MeshRenderer secretRenderer = secretChild.GetComponent<MeshRenderer>();
            if (secretRenderer != null)
            {
                secretRenderer.material = material;
                if (!string.IsNullOrEmpty(PhotoPath))
                {
                    PhotoPersistenceManager.Instance?.SavePhotoSecretMaterial(
                        PhotoPath, SecretMaterialPath);
                }
            }
        }
    }

    private Mesh CreatePhotoMesh()
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        
        int[] triangles = new int[6]
        {
            0, 2, 1,
            2, 3, 1
        };
        
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}