using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance; // Singleton to ensure only one pause menu
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    
    void Awake()
    {
        // Singleton pattern - destroy duplicates
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Duplicate PauseMenu found! Destroying: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    void Start()
    {
        // Ensure game is not paused at start
        Time.timeScale = 1f;
        GameIsPaused = false;
        
        // Make sure pause menu is hidden when game starts
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
            Debug.Log("Pause menu initialized and hidden");
        }
        else
        {
            Debug.LogError("Pause Menu UI not assigned in Inspector!");
        }
        
        // Also check if UIManager has a pause menu panel and hide it
        if (UIManager.instance != null && UIManager.instance.pauseMenuPanel != null)
        {
            UIManager.instance.pauseMenuPanel.SetActive(false);
            Debug.Log("UIManager pause menu panel also hidden");
        }
        
        // Find and destroy any old pause menu objects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // Look for old pause menu UI elements (common names)
            if (obj != pauseMenuUI && obj != gameObject && 
                (obj.name.Contains("PauseMenu") || 
                 obj.name.Contains("Pause Menu") || 
                 obj.name.Contains("OldPauseMenu")))
            {
                // Check if it's not a child of our current pause menu
                if (!obj.transform.IsChildOf(transform))
                {
                    Debug.Log("Found and destroying old pause menu object: " + obj.name);
                    Destroy(obj);
                }
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // Don't allow pausing if game has ended
        if (GameManager.instance != null)
        {
            // You could add a check here if GameManager has a gameEnded flag
        }
        
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
        
        // Also hide via UIManager if available
        if (UIManager.instance != null)
        {
            UIManager.instance.HidePauseMenu();
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
        else
        {
            Debug.LogError("Cannot show pause menu - pauseMenuUI is not assigned!");
        }
        
        // Also show via UIManager if available
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowPauseMenu();
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
    
    void OnDestroy()
    {
        // Clean up singleton reference
        if (instance == this)
        {
            instance = null;
        }
        
        // Make sure game isn't left paused
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
}