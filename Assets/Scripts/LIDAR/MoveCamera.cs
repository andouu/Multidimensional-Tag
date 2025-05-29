using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    public Transform castPointPosition;
    public Transform scannerPosition;

    void Update()
    {
        transform.position = cameraPosition.position;
        castPointPosition.position = cameraPosition.position + cameraPosition.forward;
        scannerPosition.position = cameraPosition.position + cameraPosition.forward;
    }
}
