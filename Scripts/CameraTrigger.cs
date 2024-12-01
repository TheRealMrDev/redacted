using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private EnhancedCamera targetCamera;
    [SerializeField] private bool destroyAfterPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        PSXFirstPersonController player = other.GetComponent<PSXFirstPersonController>();
        
        if (player != null && targetCamera != null)
        {
            targetCamera.OnInteract(player);
            
            if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}