using UnityEngine;

public class InteractableItem : InteractableBase
{
    [SerializeField] private string itemName = "Item";
    [SerializeField] private bool destroyOnPickup = true;
    
    public override void OnInteract(PSXFirstPersonController player)
    {
        Debug.Log($"Picked up {itemName}");
        base.OnInteract(player);
        
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}
