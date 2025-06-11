using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public string stageName;
    public int stageDurationSeconds = 0;
    public TextMeshProUGUI stageText;
    public Timer timer;
    public CameraControl cameraControl;
    public CameraControl.FollowBehavior followBehavior = CameraControl.FollowBehavior.FromBack;
    public float followDistance = 6f;

    private bool isActivated = false;
    private Material mat;
    private float opacityVel = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        mat = gameObject.GetComponent<MeshRenderer>().material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActivated && other.gameObject.CompareTag("Player"))
        {
            isActivated = true;
            stageText.text = stageName;
            timer.durationSeconds = stageDurationSeconds;
            timer.ResetTimer();
            cameraControl.followBehavior = followBehavior;
            cameraControl.followDistance = followDistance;
            cameraControl.ResetOrientation();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isActivated)
        {
            Color curr = mat.color;
            curr.a = Mathf.SmoothDamp(curr.a, 0f, ref opacityVel, 0.1f);
            mat.color = curr;
        }
    }
}
