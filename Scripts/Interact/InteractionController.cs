using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this at the top

public class InteractionController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float highlightIntensity = 0.3f;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject interactTextObject;
    private TextMeshProUGUI interactText;

    private Camera playerCamera;
    private IInteractable currentInteractable;
    private MeshRenderer currentHighlightedRenderer;
    private Dictionary<MeshRenderer, Material[]> originalMaterials = new Dictionary<MeshRenderer, Material[]>();

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        interactText = interactTextObject.GetComponent<TextMeshProUGUI>();
        interactTextObject.SetActive(false);
        
    }

    private void Update()
    {
        HandleInteractionRaycast();
        HandleInteractionInput();
    }

    private void HandleInteractionRaycast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, interactionLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && Vector3.Distance(transform.position, hit.transform.position) <= interactable.GetInteractionDistance())
            {
                if (currentInteractable != interactable)
                {
                    UnhighlightCurrentObject();
                    currentInteractable = interactable;
                    HighlightObject(hit.collider.gameObject);
                    ShowInteractionText(interactable.GetInteractionPrompt());
                }
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private void HandleInteractionInput()
    {
        if (currentInteractable != null && Input.GetKeyDown(interactKey))
        {
            currentInteractable.OnInteract(GetComponent<PSXFirstPersonController>());
        }
    }

    private void HighlightObject(GameObject obj)
    {
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            if (!originalMaterials.ContainsKey(renderer))
            {
                originalMaterials[renderer] = renderer.materials;
            }

            Material[] highlightedMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                highlightedMaterials[i] = new Material(renderer.materials[i]);
                highlightedMaterials[i].EnableKeyword("_EMISSION");
                highlightedMaterials[i].SetColor("_EmissionColor", 
                    highlightedMaterials[i].color * highlightIntensity);
            }

            renderer.materials = highlightedMaterials;
            currentHighlightedRenderer = renderer;
        }
    }

    private void UnhighlightCurrentObject()
    {
        if (currentHighlightedRenderer != null && originalMaterials.ContainsKey(currentHighlightedRenderer))
        {
            currentHighlightedRenderer.materials = originalMaterials[currentHighlightedRenderer];
            currentHighlightedRenderer = null;
        }
    }

    private void ClearCurrentInteractable()
    {
        UnhighlightCurrentObject();
        currentInteractable = null;
        HideInteractionText();
    }

    private void ShowInteractionText(string prompt)
    {
        interactText.text = prompt;
        interactTextObject.SetActive(true);
    }

    private void HideInteractionText()
    {
        interactTextObject.SetActive(false);
    }
}
