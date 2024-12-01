using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PersistentPhotoData
{
    public string photoPath;
    public string secretMaterialPath;
}

public class PhotoPersistenceManager : MonoBehaviour
{
    public static PhotoPersistenceManager Instance { get; private set; }

    private Dictionary<string, PersistentPhotoData> persistentPhotos 
        = new Dictionary<string, PersistentPhotoData>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Save secret material path for a specific photo
    public void SavePhotoSecretMaterial(string photoPath, string secretMaterialPath)
    {
        if (string.IsNullOrEmpty(photoPath)) return;

        persistentPhotos[photoPath] = new PersistentPhotoData
        {
            photoPath = photoPath,
            secretMaterialPath = secretMaterialPath
        };
    }

    // Retrieve secret material path for a photo
    public string GetPhotoSecretMaterialPath(string photoPath)
    {
        if (persistentPhotos.TryGetValue(photoPath, out var photoData))
        {
            return photoData.secretMaterialPath;
        }
        return null;
    }
}