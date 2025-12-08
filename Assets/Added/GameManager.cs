using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [Header("Win Condition")]
    public int totalCoinsInLevel = 0; // Set this to the number of coins in your level
    private int coinsCollected = 0;
    
    private bool gameEnded = false;
    
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
        // Auto-count coins if not manually set
        if (totalCoinsInLevel == 0)
        {
            CountCoinsInLevel();
        }
        
        Debug.Log("=== GAME MANAGER INITIALIZED ===");
        Debug.Log("Total coins in level: " + totalCoinsInLevel);
        Debug.Log("=================================");
    }
    
    void CountCoinsInLevel()
    {
        // Count all GameObjects with "Coin" in their name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Coin"))
            {
                totalCoinsInLevel++;
            }
        }
        Debug.Log("Auto-detected " + totalCoinsInLevel + " coins in level");
    }
    
    public void CoinCollected()
    {
        if (gameEnded) return;
        
        coinsCollected++;
        Debug.Log("=== COIN COLLECTED ===");
        Debug.Log("Coins collected: " + coinsCollected + "/" + totalCoinsInLevel);
        Debug.Log("======================");
        
        // Check if all coins collected
        if (coinsCollected >= totalCoinsInLevel)
        {
            Debug.Log("ALL COINS COLLECTED! TRIGGERING WIN!");
            WinGame();
        }
    }
    
    public void CheckWinCondition(int currentScore)
    {
        // This method kept for compatibility but not used for coin-based win
        // You can remove this if you want
    }
    
    public void WinGame()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("=== YOU WIN! ===");
        Debug.Log("All " + totalCoinsInLevel + " coins collected!");
        Debug.Log("================");
        
        // Stop level music and play victory sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.FadeOutMusic(1.5f); // Fade out over 1.5 seconds
            AudioManager.instance.PlayVictory();
            Debug.Log("Playing victory sound!");
        }
        
        // Show win UI
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowWinMessage();
            Debug.Log("Win message displayed!");
        }
        else
        {
            Debug.LogError("UIManager.instance is NULL! Cannot show win message!");
        }
        
        // Pause game
        Time.timeScale = 0f;
    }
    
    public void LoseGame()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("=== GAME OVER - YOU LOSE! ===");
        
        // Stop level music and play game over sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
            AudioManager.instance.PlayGameOver();
            Debug.Log("Playing game over sound!");
        }
        
        // Show game over UI
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowGameOverMessage();
        }
        
        // Pause game
        Time.timeScale = 0f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        // Restart music when scene reloads
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // Stop music before quitting
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopMusic();
        }
        
        Application.Quit();
    }
    
    // Helper methods
    public int GetCoinsCollected()
    {
        return coinsCollected;
    }
    
    public int GetTotalCoins()
    {
        return totalCoinsInLevel;
    }
}