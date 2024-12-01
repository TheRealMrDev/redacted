using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    string GetInteractionPrompt();
    void OnInteract(PSXFirstPersonController player);
    float GetInteractionDistance();
}

public abstract class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected string interactionPrompt = "Press E to interact";
    [SerializeField] protected float interactionDistance = 2f;
    [SerializeField] protected bool canInteractMultipleTimes = true;
    [SerializeField] protected UnityEvent onInteracted;

    public virtual string GetInteractionPrompt() => interactionPrompt;
    public virtual float GetInteractionDistance() => interactionDistance;
    

    public virtual void OnInteract(PSXFirstPersonController player)
    {
        onInteracted?.Invoke();
        if (!canInteractMultipleTimes)
        {
            enabled = false;
        }
    }
}