using System.Collections.Generic;
using UnityEngine;

public class PhotoManager : MonoBehaviour
{
    public static PhotoManager Instance { get; private set; }
    
    public List<PhotoData> capturedPhotos = new List<PhotoData>();
    
    [System.Serializable]
    public class PhotoData
    {
        public Texture2D photoTexture;
        public PhotoMetadata metadata;
        public string photoPath;
    }
    
    private void Awake()
    {
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
    
    public void AddPhoto(Texture2D photo, PhotoMetadata metadata, string photoPath)
    {
        capturedPhotos.Add(new PhotoData 
        { 
            photoTexture = photo, 
            metadata = metadata, 
            photoPath = photoPath 
        });
    }
    
    public void TransferPhotosToPhotoBoard(PhotoBoard targetPhotoBoard)
    {
        foreach (var photoData in capturedPhotos)
        {
            targetPhotoBoard.AddPhoto(photoData.photoTexture, photoData.metadata, photoData.photoPath);
        }
        
        capturedPhotos.Clear();
    }
}