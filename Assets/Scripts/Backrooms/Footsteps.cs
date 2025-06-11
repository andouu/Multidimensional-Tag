using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource footstepSound;

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey && Time.timeScale != 0f)
        {
            footstepSound.enabled = true;
        }
        else 
        {
            footstepSound.enabled = false;
        }
    }
}
