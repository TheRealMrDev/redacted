using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class PhotoMetadata
{
    public DateTime timestamp;
    public float aperture;
    public float shutterSpeed;
    public float focalLength;
    public float focusDistance;
    public float exposureValue;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
}
public class PhotoBoard : InteractableBase
{
    public static PhotoBoard Instance { get; private set; }
    
    [Header("Board Settings")]
    [SerializeField] private Vector2 boardSize = new Vector2(2f, 1.5f);
    [SerializeField] private float pinDistance = 0.1f;
    [SerializeField] private GameObject photoPrefab;
    [SerializeField] private Transform photosParent;
    [SerializeField] private Material photoMaterial;
    [SerializeField] private Material pinnedPhotoMaterial; // Material for pinned photos
    [SerializeField] private float photoScale = 0.2f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float zoomSpeed = 0.5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private float highlightBrightness = 1.2f; // Brightness multiplier for highlighted photos
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverTransitionSpeed = 5f;
    [SerializeField] private float highlightTransitionSpeed = 8f; // Speed of brightness transition
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;
    [SerializeField] private AudioClip pinSound;
    
    [Header("Camera Settings")]
    [SerializeField] private float cameraTransitionSpeed = 5f;
    [SerializeField] private float viewDistance = 2f;
    [SerializeField] private float viewAngle = 45f;
    [SerializeField] private float viewHeight = 1.5f;
    [SerializeField] private float zoomViewDistance = 1f;

    [Header("UI Settings")]
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private GameObject photoInfoPanel; // Added for metadata display
    
    [Header("Organization Features")]
    [SerializeField] private int maxColumns = 4;
    [SerializeField] private float photoSpacing = 0.1f;
    
    [Header("Examination Settings")]
    [SerializeField] private float examineDistance = 0.5f;
    [SerializeField] private float examineHeight = 1.6f;
    [SerializeField] private float examineRotationSpeed = 100f;
    [SerializeField] private float examineSmoothSpeed = 10f;
    [SerializeField] private Vector3 examineOffset = new Vector3(0f, 0f, -0.5f);

    
    private string currentInputCode = "";
    private float secretDisplayTimer;
    private bool isExamining = false;
    private Vector3 originalPhotoPosition;
    private Quaternion originalPhotoRotation;
    private Vector3 examinePosition;
    private float currentExamineRotationX;
    private float currentExamineRotationY;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private List<PhotoOnBoard> photosOnBoard = new List<PhotoOnBoard>();
    private PhotoOnBoard selectedPhoto;
    private Vector3 dragOffset;
    private Camera mainCamera;
    private AudioSource audioSource;
    private bool isInteracting;
    private bool isZooming;
    private PSXFirstPersonController currentPlayer;
    
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform cameraTransform;
    private bool isTransitioning;
    private float transitionTime;
    [SerializeField] private float dragSmoothSpeed = 15f; // Controls how smooth the dragging feels
    private Vector3 targetDragPosition;
    private bool isDragging;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        audioSource = gameObject.AddComponent<AudioSource>();
        interactionPrompt = "Examine";
    }


    private void Update()
    {
        if (isTransitioning)
        {
            UpdateCameraTransition();
            return;
        }
        
        if (!isInteracting)
            return;

        UpdatePhotoInteraction();
    }
    private void UpdatePhotoInteraction()
    {
        // Only allow Tab when not examining
        if (Input.GetKeyDown(KeyCode.Tab) && !isExamining)
        {
            ArrangePhotosInGrid();
            PlaySound(dropSound);
            return;
        }

        if (isExamining)
        {
            HandlePhotoExamination();
            return;
        }

        if (selectedPhoto != null)
        {
            if (Input.GetKey(KeyCode.Q))
                selectedPhoto.transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
            if (Input.GetKey(KeyCode.E))
                selectedPhoto.transform.Rotate(Vector3.forward * (-rotationSpeed * Time.deltaTime));
        }

        HandlePhotoSelection();
        HandlePhotoDragging();
    
        // Change examination trigger to X key
        if (Input.GetKeyDown(KeyCode.X) && selectedPhoto != null && !isExamining)
        {
            StartPhotoExamination();
        }
    }

    private void StartPhotoExamination()
    {
        isExamining = true;
        isDragging = false;
        
        
        originalPhotoPosition = selectedPhoto.transform.position;
        originalPhotoRotation = selectedPhoto.transform.rotation;
        
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraPosition = mainCamera.transform.position;
        examinePosition = cameraPosition + (cameraForward * examineDistance) + examineOffset;
        
        currentExamineRotationX = 0f;
        currentExamineRotationY = 0f;
    }

    private void HandlePhotoExamination()
    {
        if (selectedPhoto == null)
        {
            EndPhotoExamination();
            return;
        }

        // Exit examination mode
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
        {
            EndPhotoExamination();
            return;
        }
        
        // Update rotation based on mouse movement while holding right mouse button
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * examineRotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * examineRotationSpeed * Time.deltaTime;

            currentExamineRotationX += mouseY;
            currentExamineRotationY += mouseX;
            
            // Clamp vertical rotation to prevent flipping
            currentExamineRotationX = Mathf.Clamp(currentExamineRotationX, -80f, 80f);
        }

        // Update photo position and rotation
        Vector3 targetPosition = mainCamera.transform.position + 
                               mainCamera.transform.forward * examineDistance +
                               examineOffset;

        Quaternion targetRotation = Quaternion.Euler(currentExamineRotationX, currentExamineRotationY, 0f);

        // Smooth movement and rotation
        selectedPhoto.transform.position = Vector3.Lerp(
            selectedPhoto.transform.position,
            targetPosition,
            Time.deltaTime * examineSmoothSpeed
        );

        selectedPhoto.transform.rotation = Quaternion.Lerp(
            selectedPhoto.transform.rotation,
            targetRotation,
            Time.deltaTime * examineSmoothSpeed
        );
    }
    


    
    private void EndPhotoExamination(bool immediate = false)
    {
        if (!isExamining || selectedPhoto == null)
            return;

        isExamining = false;

        if (immediate)
        {
            selectedPhoto.transform.rotation = originalPhotoRotation;
            currentExamineRotationX = 0f;
            currentExamineRotationY = 0f;
            selectedPhoto = null;
        }
        else
        {
            StartCoroutine(SmoothReturnToBoard());
        }
    }
    private System.Collections.IEnumerator SmoothReturnToBoard()
    {
        Vector3 startPos = selectedPhoto.transform.position;
        Quaternion startRot = selectedPhoto.transform.rotation;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
    
            t = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            selectedPhoto.transform.position = Vector3.Lerp(startPos, originalPhotoPosition, t);
            selectedPhoto.transform.rotation = Quaternion.Lerp(startRot, originalPhotoRotation, t);
    
            yield return null;
        }

        selectedPhoto.transform.position = originalPhotoPosition;
        selectedPhoto.transform.rotation = originalPhotoRotation;
    
        currentExamineRotationX = 0f;
        currentExamineRotationY = 0f;

        selectedPhoto = null;
        isDragging = false;
    }

    private void Start()
    {
        mainCamera = Camera.main;
        cameraTransform = mainCamera.transform;
        
        if (photosParent == null)
        {
            photosParent = new GameObject("Photos").transform;
            photosParent.parent = transform;
            photosParent.localPosition = Vector3.zero;
            photosParent.localRotation = Quaternion.identity;
        }

        if (photoPrefab != null)
        {
            photoPrefab.transform.localScale = Vector3.one * photoScale;
        }
        
        if (PhotoManager.Instance != null)
        {
            PhotoManager.Instance.TransferPhotosToPhotoBoard(GetComponent<PhotoBoard>());
        }
    }
    
   
    private void ArrangePhotosInGrid()
    {
        float startX = -boardSize.x / 2f + photoSpacing;
        float startY = boardSize.y / 2f - photoSpacing;
        float currentX = startX;
        float currentY = startY;
        int currentColumn = 0;

        foreach (var photo in photosOnBoard)
        {
            Vector3 targetPosition = new Vector3(currentX, currentY, -pinDistance);
            photo.transform.localPosition = targetPosition;
            photo.transform.localRotation = Quaternion.Euler(0f, 0f, 0f); 
            photo.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            photo.transform.localScale = Vector3.one * photoScale;
        
            currentColumn++;
            currentX += photoScale + photoSpacing;
        
            if (currentColumn >= maxColumns)
            {
                currentColumn = 0;
                currentX = startX;
                currentY -= photoScale + photoSpacing;
            }
        }
    }
    
    public override void OnInteract(PSXFirstPersonController player)
    {
        base.OnInteract(player);
        Debug.Log("Board interaction triggered");
    
        if (isExamining)
        {
            Debug.Log("Cannot exit PhotoBoard while examining a photo");
            return;
        }
    
        isInteracting = !isInteracting;
        currentPlayer = player;
    
        if (isInteracting)
        {
            originalCameraPosition = cameraTransform.position;
            originalCameraRotation = cameraTransform.rotation;
        
            if (currentPlayer != null)
            {
                currentPlayer.enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            
                if (playerHUD != null)
                {
                    playerHUD.SetActive(false);
                }
            
                isTransitioning = true;
                transitionTime = 0f;
                Debug.Log("Entered board interaction mode");
            }
        }
        else
        {
            isTransitioning = true;
            transitionTime = 0f;
        
            if (playerHUD != null)
            {
                playerHUD.SetActive(true);
            }
        
            if (currentPlayer != null)
            {
                StartCoroutine(ReturnToPlayerControl());
                Debug.Log("Exited board interaction mode");
            }
        }
    }
    private System.Collections.IEnumerator ReturnToPlayerControl()
    {
        while (isTransitioning)
        {
            yield return null;
        }
        
        currentPlayer.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        if (playerHUD != null)
        {
            playerHUD.SetActive(true);
        }
    }

    private void UpdateCameraTransition()
    {
        transitionTime += Time.deltaTime * cameraTransitionSpeed;
        float t = Mathf.Clamp01(transitionTime);
        
        if (isInteracting)
        {
            Vector3 boardCenter = transform.position;
            Vector3 boardForward = transform.forward;
            Vector3 targetPosition = boardCenter - (boardForward * viewDistance) + (Vector3.up * viewHeight);
            Quaternion targetRotation = Quaternion.LookRotation(boardCenter - targetPosition);
            
            cameraTransform.position = Vector3.Lerp(originalCameraPosition, targetPosition, t);
            cameraTransform.rotation = Quaternion.Lerp(originalCameraRotation, targetRotation, t);
        }
        else
        {
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, originalCameraPosition, t);
            cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, originalCameraRotation, t);
        }
        
        if (t >= 1f)
        {
            isTransitioning = false;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


   
    public void AddPhoto(Texture2D photo, PhotoMetadata metadata, string path)
    {
        Debug.Log("Adding photo to board");
        
        GameObject photoObj = Instantiate(photoPrefab, photosParent);
        PhotoOnBoard photoComponent = photoObj.AddComponent<PhotoOnBoard>();
        
        Material newMaterial = new Material(photoMaterial);
        newMaterial.mainTexture = photo;
        
        MeshRenderer renderer = photoObj.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = photoObj.AddComponent<MeshRenderer>();
            
        renderer.material = newMaterial;
        
        photoComponent.Initialize(newMaterial, metadata, path);
        
        Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(-boardSize.x/2f, boardSize.x/2f),
            UnityEngine.Random.Range(-boardSize.y/2f, boardSize.y/2f),
            -pinDistance
        );
        
        photoObj.transform.localPosition = randomPosition;
        photoObj.transform.localRotation = Quaternion.identity;
        photoObj.transform.localScale = Vector3.one * photoScale;
        
        photosOnBoard.Add(photoComponent);
        
        Debug.Log($"Photo added at position: {randomPosition}");
    }
    
   private void HandlePhotoDragging()
    {
        if (selectedPhoto == null)
            return;
            
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane boardPlane = new Plane(transform.forward, transform.position + (transform.forward * pinDistance));
        
        float distance;
        if (boardPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 newPosition = hitPoint + dragOffset;
            
            Vector3 localPos = transform.InverseTransformPoint(newPosition);
            localPos.x = Mathf.Clamp(localPos.x, -boardSize.x/2f, boardSize.x/2f);
            localPos.y = Mathf.Clamp(localPos.y, -boardSize.y/2f, boardSize.y/2f);
            localPos.z = -pinDistance;
            
            Vector3 proposedWorldPos = transform.TransformPoint(localPos);
            bool wouldOverlap = CheckForPhotoOverlap(proposedWorldPos, selectedPhoto);
            
            if (!wouldOverlap)
            {
                targetDragPosition = proposedWorldPos;
                isDragging = true;
            }
        }
        
        if (isDragging && selectedPhoto != null)
        {
            selectedPhoto.transform.position = Vector3.Lerp(
                selectedPhoto.transform.position,
                targetDragPosition,
                Time.deltaTime * dragSmoothSpeed
            );
            
            Vector3 currentPos = selectedPhoto.transform.position;
            currentPos.z = transform.position.z - pinDistance;
            selectedPhoto.transform.position = currentPos;
        }
    }

    private void HandlePhotoSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                PhotoOnBoard photo = hit.collider.GetComponent<PhotoOnBoard>();
                
                if (photo == null)
                {
                    photo = hit.collider.GetComponentInParent<PhotoOnBoard>();
                }
                
                if (photo != null)
                {
                    selectedPhoto = photo;
                    isDragging = true;
                    targetDragPosition = photo.transform.position;
                    
                    dragOffset = selectedPhoto.transform.position - hit.point;
                    Debug.Log("Photo selected");
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && selectedPhoto != null)
        {
            selectedPhoto = null;
            isDragging = false;
            Debug.Log("Photo deselected");
        }
    }
    

private bool CheckForPhotoOverlap(Vector3 proposedPosition, PhotoOnBoard currentPhoto)
{
    float overlapThreshold = photoScale * 0.8f;
        
    foreach (PhotoOnBoard photo in photosOnBoard)
    {
        if (photo == currentPhoto)
            continue;
                
        float distance = Vector2.Distance(
            new Vector2(proposedPosition.x, proposedPosition.y),
            new Vector2(photo.transform.position.x, photo.transform.position.y)
        );
            
        if (distance < overlapThreshold)
        {
            return true;
        }
    }
        
    return false;
}
}