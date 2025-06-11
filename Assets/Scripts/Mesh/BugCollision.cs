using UnityEngine;
using UnityEngine.SceneManagement;

public class BugCollision : MonoBehaviour
{
    public GameObject winText;
    public GameObject nextButton;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Time.timeScale = 0;
            winText.SetActive(true);
            nextButton.SetActive(true);
        }
    }

    public void OnContinue()
    {
        print("Continue button pressed.");
        SceneManager.LoadScene("LIDAR");
        Time.timeScale = 1;
    }

    public void Reset()
    {
        print("Reset button pressed.");
        SceneManager.LoadScene("Mesh");
        Time.timeScale = 1;
    }
}
