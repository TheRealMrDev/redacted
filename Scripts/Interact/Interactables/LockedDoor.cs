using UnityEngine;

public class LockedDoor : InteractableDoor
{
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeyID;

    public override void OnInteract(PSXFirstPersonController player)
    {
        Key playerKey = player.GetComponent<InventoryController>()?.FindKey(requiredKeyID);

        if (isLocked && playerKey != null)
        {
            isLocked = false;
            interactionPrompt = "Unlock";
            
            playerKey.Use();
        }
        
        if (!isLocked)
        {
            base.OnInteract(player);
        }
        else
        {
            interactionPrompt = "Locked";
        }
    }

    public override string GetInteractionPrompt()
    {
        return isLocked ? "Locked" : base.GetInteractionPrompt();
    }
}