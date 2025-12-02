using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    
    void Start()
    {
        // Make sure pause menu is hidden when game starts
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Ensure game is not paused at start
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    
    public void Resume()
    {
        Debug.Log("Game Resumed");
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
        
        // Unfreeze time
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    
    public void Pause()
    {
        Debug.Log("Game Paused");
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }
        
        // Freeze time completely
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
    
    public void QuitToMainMenu()
    {
        Debug.Log("Returning to Main Menu");
        
        // Unfreeze time before loading new scene
        Time.timeScale = 1f;
        GameIsPaused = false;
        
        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }
}