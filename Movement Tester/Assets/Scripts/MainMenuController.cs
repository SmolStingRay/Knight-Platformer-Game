using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string firstLevelSceneName = "HealthSystemTest";
    [SerializeField] private string creditsSceneName = "Credits";

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game pressed");

        Application.Quit();
    }
}