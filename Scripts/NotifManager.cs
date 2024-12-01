using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeSpeed = 2f;

    private Coroutine currentNotificationCoroutine;

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

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void ShowNotification(string message)
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
        }

        currentNotificationCoroutine = StartCoroutine(DisplayNotification(message));
    }

    private IEnumerator DisplayNotification(string message)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("Notification UI components are not assigned!");
            yield break;
        }

        notificationText.text = message;

        notificationPanel.SetActive(true);
        CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(displayDuration);

        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        notificationPanel.SetActive(false);
    }

    public void ShowCameraEquipmentNotification()
    {
        ShowNotification("Must Grab Camera");
    }

    public void ShowObjectivesNotification()
    {
        ShowNotification("Do All Objectives First");
    }
}