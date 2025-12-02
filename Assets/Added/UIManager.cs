using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    
    [Header("UI Elements")]
    public Text livesText;
    
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject gameOverPanel;
    public GameObject pauseMenuPanel; // NEW: Reference to pause menu panel
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Make sure all panels are hidden at start
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            Debug.Log("Pause menu panel hidden at start");
        }
        
        // Ensure game is not paused
        Time.timeScale = 1f;
    }
    
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives;
        }
    }
    
    public void ShowWinMessage()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
    
    public void ShowGameOverMessage()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    // NEW: Pause menu helper methods
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }
    }
    
    public void HidePauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }
    
    // Button functions
    public void RestartGame()
    {
        // Unpause game first (in case it was paused)
        Time.timeScale = 1f;
        
        // Make sure pause state is reset
        if (PauseMenu.instance != null)
        {
            PauseMenu.GameIsPaused = false;
        }
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        // Unpause game first
        Time.timeScale = 1f;
        
        // Make sure pause state is reset
        if (PauseMenu.instance != null)
        {
            PauseMenu.GameIsPaused = false;
        }
        
        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }
}