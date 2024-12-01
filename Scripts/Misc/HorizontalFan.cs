using UnityEngine;

public class HorizontalFanRotation : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public bool clockwise = true;

    void Update()
    {
        float direction = clockwise ? 1f : -1f;

        transform.Rotate(Vector3.up * rotationSpeed * direction * Time.deltaTime);
    }
}