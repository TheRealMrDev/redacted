using UnityEngine;

public class Key : InteractableBase
{
    [SerializeField] public string keyID;
    [SerializeField] public string keyName = "Key";
    
    private bool isPickedUp = false;

    public override void OnInteract(PSXFirstPersonController player)
    {
        InventoryController inventory = player.GetComponent<InventoryController>();
        if (inventory != null && !isPickedUp)
        {
            inventory.AddKey(this);
            isPickedUp = true;
            
            gameObject.SetActive(false);
        }

        base.OnInteract(player);
    }

    public void Use()
    {
        isPickedUp = false;
    }

    public override string GetInteractionPrompt()
    {
        return "Pick up " + keyName;
    }
}