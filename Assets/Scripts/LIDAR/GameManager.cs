using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        print("Restarting game...");
        Time.timeScale = 1f;
    }

    public void NextScene()
    {
        print("Switching scenees....");
        SceneManager.LoadScene("Platformer");
    }
}