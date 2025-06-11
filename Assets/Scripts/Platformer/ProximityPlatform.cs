using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ProximityPlatform : MonoBehaviour
{
    public Transform player;
    public float activationRadius = 0.5f;
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.zero;

    private bool isActivated = false;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private Vector3 posVel = Vector3.zero;
    private Vector3 scaleVel = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isActivated)
        {
            isActivated = Vector3.Distance(transform.position, player.position) <= activationRadius;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, originalPosition + positionOffset, ref posVel, 0.1f);
            transform.localScale = Vector3.SmoothDamp(transform.localScale, originalScale + scaleOffset, ref scaleVel, 0.1f);
        }
    }
}
