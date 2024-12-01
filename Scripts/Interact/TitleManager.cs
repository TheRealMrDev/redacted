using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    [System.Serializable]
    public class PinboardButton
    {
        public GameObject buttonObject;
        public ButtonType buttonType;
        public Color defaultColor;
        public Color hoverColor;
        public Vector3 originalPosition;
    }

    public enum ButtonType
    {
        Play,
        Settings,
        Credits,
        Quit,
        SettingsBack,
        CreditsBack
    }

    [Header("Menu Parents")]
    public GameObject startMenuParent;
    public GameObject settingsMenuParent;
    public GameObject creditsMenuParent;

    [Header("Menu Transition Settings")]
    public Vector3 startMenuExitDirection = Vector3.forward;
    public Vector3 settingsMenuExitDirection = Vector3.back;
    public Vector3 creditsMenuExitDirection = Vector3.right;
    public float startMenuExitDistance = 10f;
    public float settingsMenuExitDistance = 10f;
    public float creditsMenuExitDistance = 10f;
    public float menuTransitionDuration = 0.5f;

    [Header("Menu Initial Positions")]
    public Vector3 startMenuInitialOffset = Vector3.zero;
    public Vector3 settingsMenuInitialOffset = Vector3.zero;
    public Vector3 creditsMenuInitialOffset = Vector3.zero;

    [Header("Pinboard Buttons")]
    public PinboardButton[] buttons;

    [Header("Interaction Settings")]
    public float raycastDistance = 100f;

    [Header("Screen Fade Settings")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 1f;
    
    [Header("Music Settings")]
    public AudioSource titleMusic;
    public float distortionDuration = 1f;
    public float fadeOutDuration = 1f;

    private Camera mainCamera;
    private GameObject currentHoveredObject;
    private bool isSettingsOpen = false;
    private Vector3 startMenuOriginalPosition;
    private Vector3 settingsMenuOriginalPosition;
    private Vector3 creditsMenuOriginalPosition;
    private MenuState currentMenuState = MenuState.MainMenu;

    private enum MenuState
    {
        MainMenu
    }
    void Start()
    {
        mainCamera = Camera.main;

        startMenuOriginalPosition = startMenuParent.transform.position + startMenuInitialOffset;
        settingsMenuOriginalPosition = settingsMenuParent.transform.position + settingsMenuInitialOffset;
        creditsMenuOriginalPosition = creditsMenuParent.transform.position + creditsMenuInitialOffset;

        startMenuParent.transform.position = startMenuOriginalPosition;
        settingsMenuParent.transform.position = settingsMenuOriginalPosition + settingsMenuExitDirection * settingsMenuExitDistance;
        creditsMenuParent.transform.position = creditsMenuOriginalPosition + creditsMenuExitDirection * creditsMenuExitDistance;

        foreach (var button in buttons)
        {
            button.originalPosition = button.buttonObject.transform.position;
            button.defaultColor = GetObjectRenderer(button.buttonObject).material.color;
        }

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.gameObject.SetActive(false);
        }

        if (titleMusic != null)
        {
            titleMusic.Play();
        }
    }

    void Update()
    {
        HandleMouseInteraction();
    }

    void HandleMouseInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (currentHoveredObject != null)
        {
            ResetButtonColor(currentHoveredObject);
            currentHoveredObject = null;
        }

        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            PinboardButton hitButton = FindButtonByObject(hit.collider.gameObject);
            
            if (hitButton != null)
            {
                HighlightButton(hitButton);
                currentHoveredObject = hitButton.buttonObject;

                if (Input.GetMouseButtonDown(0))
                {
                    HandleButtonClick(hitButton);
                }
            }
        }
    }

    PinboardButton FindButtonByObject(GameObject obj)
    {
        return System.Array.Find(buttons, button => button.buttonObject == obj);
    }

    void HighlightButton(PinboardButton button)
    {
        Renderer renderer = GetObjectRenderer(button.buttonObject);
        if (renderer != null)
        {
            renderer.material.color = button.hoverColor;
        }
    }

    void ResetButtonColor(GameObject buttonObject)
    {
        PinboardButton button = FindButtonByObject(buttonObject);
        if (button != null)
        {
            Renderer renderer = GetObjectRenderer(buttonObject);
            if (renderer != null)
            {
                renderer.material.color = button.defaultColor;
            }
        }
    }

    void HandleButtonClick(PinboardButton button)
    {
        switch (button.buttonType)
        {
            case ButtonType.Play:
                StartCoroutine(DistortAndFadeOutMusic());
                StartCoroutine(FadeAndLoadScene("Cutscene1"));
                break;
            case ButtonType.Settings:
                StartCoroutine(TransitionToSettingsMenu());
                break;
            case ButtonType.Credits:
                StartCoroutine(TransitionToCreditsMenu());
                break;
            case ButtonType.Quit:
                QuitGame();
                break;
            case ButtonType.SettingsBack:
                StartCoroutine(TransitionToMainMenu());
                break;
            case ButtonType.CreditsBack:
                StartCoroutine(TransitionToMainMenu());
                break;
        }
    }
    IEnumerator TransitionToCreditsMenu()
    {
        float elapsed = 0f;
        Vector3 startMenuExitPosition = startMenuOriginalPosition + startMenuExitDirection * startMenuExitDistance;
        Vector3 creditsMenuEntryPosition = creditsMenuOriginalPosition;

        while (elapsed < menuTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / menuTransitionDuration;

            startMenuParent.transform.position = Vector3.Lerp(
                startMenuOriginalPosition, 
                startMenuExitPosition, 
                t
            );

            creditsMenuParent.transform.position = Vector3.Lerp(
                creditsMenuOriginalPosition + creditsMenuExitDirection * creditsMenuExitDistance,
                creditsMenuEntryPosition, 
                t
            );

            yield return null;
        }

        startMenuParent.transform.position = startMenuExitPosition;
        creditsMenuParent.transform.position = creditsMenuEntryPosition;
    }
       IEnumerator TransitionToSettingsMenu()
    {
        float elapsed = 0f;
        Vector3 startMenuExitPosition = startMenuOriginalPosition + startMenuExitDirection * startMenuExitDistance;
        Vector3 settingsMenuEntryPosition = settingsMenuOriginalPosition;

        while (elapsed < menuTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / menuTransitionDuration;

            startMenuParent.transform.position = Vector3.Lerp(
                startMenuOriginalPosition, 
                startMenuExitPosition, 
                t
            );

            settingsMenuParent.transform.position = Vector3.Lerp(
                settingsMenuOriginalPosition + settingsMenuExitDirection * settingsMenuExitDistance,
                settingsMenuEntryPosition, 
                t
            );

            yield return null;
        }

        startMenuParent.transform.position = startMenuExitPosition;
        settingsMenuParent.transform.position = settingsMenuEntryPosition;
    }

IEnumerator TransitionToMainMenu()
{
    float elapsed = 0f;
    Vector3 startMenuEntryPosition = startMenuOriginalPosition;
    
    if (settingsMenuParent.transform.position == settingsMenuOriginalPosition)
    {
        Vector3 settingsMenuExitPosition = settingsMenuOriginalPosition + settingsMenuExitDirection * settingsMenuExitDistance;

        while (elapsed < menuTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / menuTransitionDuration;

            startMenuParent.transform.position = Vector3.Lerp(
                startMenuOriginalPosition + startMenuExitDirection * startMenuExitDistance,
                startMenuEntryPosition,
                t
            );

            settingsMenuParent.transform.position = Vector3.Lerp(
                settingsMenuOriginalPosition,
                settingsMenuExitPosition,
                t
            );

            yield return null;
        }

        startMenuParent.transform.position = startMenuEntryPosition;
        settingsMenuParent.transform.position = settingsMenuExitPosition;
    }
    else if (creditsMenuParent.transform.position == creditsMenuOriginalPosition)
    {
        Vector3 creditsMenuExitPosition = creditsMenuOriginalPosition + creditsMenuExitDirection * creditsMenuExitDistance;

        while (elapsed < menuTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / menuTransitionDuration;

            startMenuParent.transform.position = Vector3.Lerp(
                startMenuOriginalPosition + startMenuExitDirection * startMenuExitDistance,
                startMenuEntryPosition,
                t
            );

            creditsMenuParent.transform.position = Vector3.Lerp(
                creditsMenuOriginalPosition,
                creditsMenuExitPosition,
                t
            );

            yield return null;
        }

        startMenuParent.transform.position = startMenuEntryPosition;
        creditsMenuParent.transform.position = creditsMenuExitPosition;
    }
}

    void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    Renderer GetObjectRenderer(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            renderer = obj.GetComponentInChildren<Renderer>();
        }
        return renderer;
    }
    
    IEnumerator FadeAndLoadScene(string sceneName)
    {
        if (fadeOverlay == null)
        {
            Debug.LogError("Fade overlay is not assigned!");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        fadeOverlay.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
        try 
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scene {sceneName}: {e.Message}");
        }
    }

    IEnumerator DistortAndFadeOutMusic()
    {
        if (titleMusic != null)
        {
            float initialPitch = titleMusic.pitch;
            float elapsed = 0f;

            while (elapsed < distortionDuration)
            {
                elapsed += Time.deltaTime;
                titleMusic.pitch = Mathf.Lerp(initialPitch, 0.5f, elapsed / distortionDuration);
                yield return null;
            }

            elapsed = 0f;
            float initialVolume = titleMusic.volume;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                titleMusic.volume = Mathf.Lerp(initialVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            titleMusic.Stop();
        }
    }
}