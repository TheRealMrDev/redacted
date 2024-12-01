using UnityEngine;
using System.Collections.Generic;

public class LightSwitch : InteractableBase
{
    [System.Serializable]
    public class EmissionObject
    {
        public GameObject gameObject;
        public Color emissionColor = Color.white;
        public float emissionIntensity = 1f;
    }
    [SerializeField] private EmissionObject[] onObjects;
    [SerializeField] private EmissionObject[] offObjects;
    [SerializeField] private bool isOn = false;
    [SerializeField] private string onPrompt = "Press E to turn OFF";
    [SerializeField] private string offPrompt = "Press E to turn ON";

    private Dictionary<MeshRenderer, Material[]> originalMaterials = new Dictionary<MeshRenderer, Material[]>();
    
    private void Start()
    {
        StoreOriginalMaterials(onObjects);
        StoreOriginalMaterials(offObjects);
        SetOppositeStates();
        interactionPrompt = isOn ? onPrompt : offPrompt;
    }

    private void StoreOriginalMaterials(EmissionObject[] objects)
    {
        if (objects == null) return;
        
        foreach (var emissionObj in objects)
        {
            if (emissionObj.gameObject != null)
            {
                MeshRenderer renderer = emissionObj.gameObject.GetComponent<MeshRenderer>();
                if (renderer != null && !originalMaterials.ContainsKey(renderer))
                {
                    Material[] originalMats = renderer.materials;
                    Material[] materialsCopy = new Material[originalMats.Length];
                    for (int i = 0; i < originalMats.Length; i++)
                    {
                        materialsCopy[i] = new Material(originalMats[i]);
                    }
                    originalMaterials[renderer] = materialsCopy;
                }
            }
        }
    }

    public override void OnInteract(PSXFirstPersonController player)
    {
        isOn = !isOn;
        SetOppositeStates();
        interactionPrompt = isOn ? onPrompt : offPrompt;
        base.OnInteract(player);
    }
    
    private void SetOppositeStates()
    {
        SetObjectStates(onObjects, isOn);
        SetObjectStates(offObjects, !isOn);
    }

    private void SetObjectStates(EmissionObject[] objects, bool active)
    {
        if (objects == null) return;

        foreach (var emissionObj in objects)
        {
            if (emissionObj?.gameObject == null) continue;

            emissionObj.gameObject.SetActive(true); 
            MeshRenderer renderer = emissionObj.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material[] materials = renderer.materials;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].EnableKeyword("_EMISSION");
                    
                    if (active)
                    {
                        materials[i].SetColor("_EmissionColor", 
                            emissionObj.emissionColor * emissionObj.emissionIntensity);
                    }
                    else
                    {
                        materials[i].SetColor("_EmissionColor", Color.black);
                    }
                }
                
                renderer.materials = materials;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value;
            }
        }
    }
}