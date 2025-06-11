using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawCutLines : MonoBehaviour
{
    public float distanceToCamera = 5f;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    private void LateUpdate()
    {
        if (GameCutter.clickCount == 1)
        {
            Vector3 mouse1 = GameCutter.firstMousePoint;
            Vector3 mouse2 = Input.mousePosition;
            Vector3 diff = (mouse2 - mouse1).normalized;
            mouse1 -= diff * 200f;
            mouse2 += diff * 200f;
            mouse1.z = distanceToCamera;
            mouse2.z = distanceToCamera;

            Vector3 point1 = Camera.main.ScreenToWorldPoint(mouse1);
            Vector3 point2 = Camera.main.ScreenToWorldPoint(mouse2);

            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, point1);
            lineRenderer.SetPosition(1, point2);
        } else
        {
            lineRenderer.positionCount = 0;
        }
    }
}
