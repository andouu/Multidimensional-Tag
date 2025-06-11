using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweenPlatform : MonoBehaviour
{
    public Vector3 posOffset;
    public float interpolationTime = 3f;

    private float t = 0f;
    private bool goTo = true;
    private Vector3 positionFrom;
    private Vector3 positionTo;
    
    // Start is called before the first frame update
    void Start()
    {
        positionFrom = transform.position;
        positionTo = transform.position + posOffset;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime / interpolationTime;
        if (t >= 1f)
        {
            t = 0f;
            goTo = !goTo;
        }

        if (goTo)
        {
            transform.position = Vector3.Lerp(positionFrom, positionTo, t);
        }
        else
        {
            transform.position = Vector3.Lerp(positionTo, positionFrom, t);
        }
    }
}
