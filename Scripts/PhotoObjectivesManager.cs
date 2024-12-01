using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PhotoObjective
{
    public string objectName;
    public GameObject targetObject;
    public float minDistance = 1f;
    public float maxDistance = 5f;
    public float minFocusAccuracy = 0.8f;
    public bool isCompleted;
    public Material secretMaterial;
    public bool isPictureRequired = true;

    [TextArea(3, 5)]
    public string description;
}

[System.Serializable]
public class LevelObjectives
{
    public string levelName;
    public List<PhotoObjective> objectives;
}

public class PhotoObjectivesManager : MonoBehaviour
{
    public static PhotoObjectivesManager Instance { get; private set; }
    
    [SerializeField] private List<LevelObjectives> levels;
    [SerializeField] private AudioClip objectiveCompleteSound;
    [SerializeField] private float minAngleToTarget = 30f;
    [SerializeField] private TMPro.TextMeshProUGUI objectivesText;
    
    private LevelObjectives currentLevel;
    private AudioSource audioSource;
    private int currentLevelIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        LoadLevel(currentLevelIndex);
    }
    
    public List<LevelObjectives> GetLevels()
    {
        return levels;
    }
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }
        
        currentLevel = levels[levelIndex];
        currentLevelIndex = levelIndex;
        
        // Reset objectives
        foreach (var objective in currentLevel.objectives)
        {
            objective.isCompleted = false;
        }
        
        UpdateObjectivesDisplay();
    }
    
public bool ValidatePhoto(Camera playerCamera, PhotoMetadata metadata)
{
    if (currentLevel == null) return false;
    
    Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    RaycastHit hit;
    
    if (Physics.Raycast(ray, out hit))
    {
        foreach (var objective in currentLevel.objectives)
        {
            if (objective.isCompleted || hit.collider.gameObject != objective.targetObject)
                continue;
            
            if (!objective.isPictureRequired)
            {
                objective.isCompleted = true;
                UpdateObjectivesDisplay();
                CheckLevelCompletion();
                return true;
            }
            
            float distance = Vector3.Distance(playerCamera.transform.position, hit.point);
            if (distance < objective.minDistance || distance > objective.maxDistance)
                continue;
            
            Vector3 directionToTarget = (hit.point - playerCamera.transform.position).normalized;
            float angle = Vector3.Angle(playerCamera.transform.forward, directionToTarget);
            if (angle > minAngleToTarget)
                continue;
            
            float focusAccuracy = 1f - Mathf.Abs(1f - (hit.distance / metadata.focusDistance));
            if (focusAccuracy < objective.minFocusAccuracy)
                continue;
            
            objective.isCompleted = true;
            
            Transform secretChild = hit.collider.gameObject.transform.Find("Secret");
            if (secretChild != null && objective.secretMaterial != null)
            {
                MeshRenderer secretRenderer = secretChild.GetComponent<MeshRenderer>();
                if (secretRenderer != null)
                {
                    secretRenderer.material = objective.secretMaterial;
                }
            }
            
            if (objectiveCompleteSound != null)
                audioSource.PlayOneShot(objectiveCompleteSound);
            
            UpdateObjectivesDisplay();
            CheckLevelCompletion();
            return true;
        }
    }
    
    return false;
}
     
private void UpdateObjectivesDisplay()
{
    if (objectivesText == null) return;

    System.Text.StringBuilder sb = new System.Text.StringBuilder();

    sb.AppendLine($"<size=150%><b><color=black>{currentLevel.levelName}</color></b></size>");
    sb.AppendLine();

    foreach (var objective in currentLevel.objectives)
    {
        if (!objective.isPictureRequired)
            continue;

        string statusIndicator = objective.isCompleted ? 
            "<color=#2ecc71>[X]</color>" : 
            "<color=#e74c3c>[ ]</color>";  

        string objectiveText = objective.isCompleted ?
            $"<color=#95a5a6><s>{objective.description}</s></color>" :
            $"<color=#34495e>{objective.description}</color>"; 

        sb.AppendLine($"{statusIndicator} {objectiveText}");
    }

    objectivesText.text = sb.ToString();
}
private void CheckLevelCompletion()
{
    bool allRequiredCompleted = currentLevel.objectives
        .Where(obj => obj.isPictureRequired)
        .All(obj => obj.isCompleted);

    if (allRequiredCompleted)
    {
        Debug.Log($"Level {currentLevel.levelName} completed!");
    }
}
}