using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LIDAR
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform cameraTransform;
        public Transform castPointTransform;
        public Transform scannerTransform;

        void Update()
        {
            Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
            transform.position = cameraTransform.position;
            castPointTransform.position = cameraTransform.position;// + dir*2f;
            scannerTransform.position = castPointTransform.position;
        }
    }
}
