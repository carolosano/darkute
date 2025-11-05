using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup pauseMenuCanvas;

    [Header("Escenas")]
    [SerializeField] private string introHouseScene = "IntroHouse";
    [SerializeField] private string mainMenuScene = "Menu";

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.gameObject.SetActive(true);
            pauseMenuCanvas.alpha = 1f;
            pauseMenuCanvas.interactable = true;
            pauseMenuCanvas.blocksRaycasts = true;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.alpha = 0f;
            pauseMenuCanvas.interactable = false;
            pauseMenuCanvas.blocksRaycasts = false;
            pauseMenuCanvas.gameObject.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(introHouseScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}




