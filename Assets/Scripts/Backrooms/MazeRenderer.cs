using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField] private MazeGenerator mazeGenerator;
    [SerializeField] private GameObject MazeCellPrefab;
    [SerializeField] private GameObject DoorPrefab; // The door object to place in the exit cell

    [Header("Win Screen UI")]
    [SerializeField] private GameObject winPanel; // Assign your win panel here
    [SerializeField] private TextMeshProUGUI winText; // Assign your TMPPro text here

    public float CellSize = 1f;

    private void Start()
    {
        // Make sure win panel is hidden at start
        if (winPanel != null)
            winPanel.SetActive(false);

        MazeCell[,] maze = mazeGenerator.GetMaze();
        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                GameObject newCell = Instantiate(MazeCellPrefab, new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, transform);

                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;
                bool right = false;
                bool bottom = false;

                // Keep all walls intact, including for exit cells
                if (x == mazeGenerator.mazeWidth - 1) right = true;
                if (y == 0) bottom = true;

                mazeCell.Init(top, bottom, right, left);
            }
        }

        // Place door in the exit cell
        if (mazeGenerator.exitX >= 0 && mazeGenerator.exitY >= 0)
        {
            PlaceDoor();
        }
    }

    void PlaceDoor()
    {
        Vector3 doorPosition = new Vector3(
            mazeGenerator.exitX * CellSize,
            0f,
            mazeGenerator.exitY * CellSize
        );

        Quaternion doorRotation = Quaternion.identity;

        // Determine which wall to place the door in and adjust position/rotation
        // Place door in the outer wall of the exit cell
        if (mazeGenerator.exitX == mazeGenerator.mazeWidth - 1)
        {
            // Exit on right edge - place door in right wall
            doorPosition.x += CellSize / 2f - 0.03f;
            doorRotation = Quaternion.Euler(0, 90, 0);
            Debug.Log("Placing door in right wall");
        }
        else if (mazeGenerator.exitX == 0)
        {
            // Exit on left edge - place door in left wall
            doorPosition.x -= CellSize / 2f;
            doorRotation = Quaternion.Euler(0, -90, 0);
            Debug.Log("Placing door in left wall");
        }
        else if (mazeGenerator.exitY == mazeGenerator.mazeHeight - 1)
        {
            // Exit on top edge - place door in top wall
            doorPosition.z += CellSize / 2f - 0.03f;
            doorRotation = Quaternion.Euler(0, 0, 0);
            Debug.Log("Placing door in top wall");
        }
        else if (mazeGenerator.exitY == 0)
        {
            // Exit on bottom edge - place door in bottom wall
            doorPosition.z -= CellSize / 2f;
            doorRotation = Quaternion.Euler(0, 180, 0);
            Debug.Log("Placing door in bottom wall");
        }

        GameObject door;


        door = Instantiate(DoorPrefab, doorPosition, doorRotation);
        Collider doorCollider = door.GetComponent<Collider>();
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
            Debug.Log("Set existing collider as trigger");
        }
        else
        {
            // Add a trigger collider if none exists
            BoxCollider newTrigger = door.AddComponent<BoxCollider>();
            newTrigger.isTrigger = true;
            newTrigger.size = new Vector3(0.7f, 1.9f, 0.5f);
            Debug.Log("Added new trigger collider");
        }


        // Ensure the door has a trigger
        if (door.GetComponent<Collider>() != null)
        {
            door.GetComponent<Collider>().isTrigger = true;
        }

        // Add exit trigger if it doesn't have one
        if (door.GetComponent<ExitTrigger>() == null)
        {
            ExitTrigger exitTrigger = door.AddComponent<ExitTrigger>();
            // Pass the UI references to the exit trigger
            exitTrigger.SetUIReferences(winPanel, winText);
        }
    }
}

// Simple exit trigger component
public class ExitTrigger : MonoBehaviour
{
    private GameObject winPanel;
    private TextMeshProUGUI winText;
    private bool hasTriggered = false; // Prevent multiple triggers

    public void SetUIReferences(GameObject panel, TextMeshProUGUI text)
    {
        winPanel = panel;
        winText = text;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Maze Complete! You found the exit!");

            // Pause the game
            Time.timeScale = 0f;
            AudioListener.pause = true;
            // Show win panel
            if (winPanel != null)
            {
                winPanel.SetActive(true);
                Debug.Log("Win panel shown");
            }
            else
            {
                Debug.LogWarning("Win panel not assigned!");
            }
        }
    }
}