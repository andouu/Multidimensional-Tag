using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bee : MonoBehaviour
{
    public float rotSpeed = 30f;
    public float dY = 1f;
    public float floatSpeed = 1.5f;

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetPos = transform.position;
        targetPos.y = Mathf.Sin(Time.time * floatSpeed) * dY + originalPosition.y;
        transform.position = targetPos;

        Vector3 dir = transform.eulerAngles;
        dir.y += rotSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(dir);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Util.NextLevel();
        }
    }
}
