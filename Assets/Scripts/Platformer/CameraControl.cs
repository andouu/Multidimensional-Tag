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
        {
            FollowBehavior.FromFront,
            (theta) => new Vector3(
                0f,
                Mathf.Sin(theta * Mathf.Deg2Rad),
                Mathf.Cos(theta * Mathf.Deg2Rad)
            ).normalized
        }
    };

    private FollowBehavior _followBehavior = FollowBehavior.FromBack;
    private Vector3 _followVelocity = Vector3.zero;

    private void ResetOrientation()
    {
        cam.transform.position = CalculateCameraPosition();
        cam.transform.LookAt(followTarget);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        ResetOrientation();
    }

    private Vector3 CalculateCameraPosition()
    {
        Vector3 dir = _followDirections[_followBehavior](followAngle);
        return followTarget.position + dir * followDistance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Move Camera to desired position and then look at the player
        Vector3 targetCamPos = CalculateCameraPosition();
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetCamPos, ref _followVelocity, followTime * Time.fixedDeltaTime);
        cam.transform.LookAt(followTarget);
    }
}
