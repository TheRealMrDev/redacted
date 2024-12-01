using UnityEngine;

public class VerticalFanRotation : MonoBehaviour
{
    public float rotationSpeed = 50f;
    public bool clockwise = true;

    void Update()
    {
        float direction = clockwise ? 1f : -1f;

        transform.Rotate(Vector3.right * rotationSpeed * direction * Time.deltaTime);
    }
}