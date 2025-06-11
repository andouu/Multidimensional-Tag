using UnityEngine;

public class BugMovement : MonoBehaviour
{
    public Transform HeadBase;
    public Transform Wing1Base;
    public Transform Wing2Base;

    private float t = 0f;

    void Update()
    {
        t += Time.deltaTime;
        HeadBase.rotation = Quaternion.Euler(0f, 20f * Mathf.Sin(3f * t), 0f);
        Wing1Base.rotation = Quaternion.Euler(30f + 30f * Mathf.Sin(4f * t), 0f, 0f);
        Wing2Base.rotation = Quaternion.Euler(-30f + 30f * Mathf.Cos(4f * t), 0f, 0f);
    }
}
