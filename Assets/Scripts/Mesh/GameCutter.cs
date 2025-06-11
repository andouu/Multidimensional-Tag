using UnityEngine;

public class GameCutter : MonoBehaviour
{
    public static Vector3 firstMousePoint;
    public static int clickCount = 0;

    private Vector3[] directions = new Vector3[2];

    private void OnMouseDown()
    {
        // Get mouse position on object
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Meshcut
        if (clickCount == 0)
        {
            firstMousePoint = Input.mousePosition;
            directions[0] = ray.direction;
            clickCount++;
        }
        else
        {
            directions[1] = ray.direction;
            Vector3 normal = Vector3.Cross(directions[0], directions[1]);
            if (normal.y > 0f) normal *= -1f;
            Plane cutPlane = new Plane(normal, ray.origin);
            GameObject newObject = MeshCutter.MeshCutter.Cut(gameObject, cutPlane);

            //gameObject.GetComponent<Rigidbody>().isKinematic = false;
            newObject.GetComponent<Rigidbody>().isKinematic = false;
            //cutBody.AddForce(100f * normal, ForceMode.Impulse);
            newObject.transform.position += normal * 1.5f;

            clickCount = 0;
        }
    }
}
