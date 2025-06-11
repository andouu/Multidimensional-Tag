using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public GameObject winText;
    public GameObject continueButton;
    private void Start()
    {
        winText.SetActive(false);
        continueButton.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        print("hit1");
        if (other.CompareTag("Player"))
        {
            print("Player touched the exit door!");
            // Do something, like load the next level
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            // Teleport player outside of scene to not see raycast
            player.position = new Vector3(100f, 100f, 100f); // Teleport player outside of scene
            winText.SetActive(true);
            continueButton.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            print("Setting timescale 0 from here2");
            Time.timeScale = 0f;
            // Clear Raycast
            // Show You Win text + button
        }
    }
}
