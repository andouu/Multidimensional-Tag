using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float cameraY = 4.0f;
    public float distance = 18.0f;
    public float rotationSpeed = 8.0f;

    private float currentAngle = 0.0f;

    void Update()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        currentAngle += mouseX * rotationSpeed;
        float radians = currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Sin(radians) * distance, cameraY, Mathf.Cos(radians) * distance);
        transform.position = target.position + offset;

        transform.LookAt(target);
    }
}
