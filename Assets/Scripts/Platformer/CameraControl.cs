using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;
    public float followDistance = 6f;
    public float followTime = 0.1f;
    public float followAngle = 30f;
    
    private enum FollowBehavior
    {
        FromBack,
        FromFront,
        ExactlyUnderneath
    }
    
    private Dictionary<FollowBehavior, Func<float, Vector3>> _followDirections = new Dictionary<FollowBehavior, Func<float, Vector3>>(){
        {
            FollowBehavior.FromBack,
            (theta) => new Vector3(
                0f,
                Mathf.Sin((90f + (90f - theta)) * Mathf.Deg2Rad),
                Mathf.Cos((90f + (90f - theta)) * Mathf.Deg2Rad)
            ).normalized
        },
    };

    private FollowBehavior _followBehavior = FollowBehavior.FromBack;
    private Vector3 _followVelocity = Vector3.zero;
    private Vector3 _rotationVelocity = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    private Vector3 CalculateCameraPosition()
    {
        Vector3 dir = _followDirections[_followBehavior](followAngle);
        Debug.DrawRay(followTarget.position, dir * 10f, Color.red);
        return followTarget.position + dir * followDistance;
    }

    private Quaternion CalculateCameraRotation()
    {
        Vector3 targetToCamera = followTarget.position - cam.transform.position;
        return Quaternion.LookRotation(targetToCamera);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Move Camera to desired position and then look at the player
        Vector3 targetCamPos = CalculateCameraPosition();
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetCamPos, ref _followVelocity, followTime);
        
        cam.transform.LookAt(followTarget);
    }
}
