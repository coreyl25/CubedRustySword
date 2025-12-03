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
    }
    
    public void CoinCollected()
    {
        if (gameEnded) return;
        
        coinsCollected++;
        Debug.Log("Coins collected: " + coinsCollected + "/" + totalCoinsInLevel);
        
        // Check if all coins collected
        if (coinsCollected >= totalCoinsInLevel)
        {
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
        Debug.Log("YOU WIN! All coins collected!");
        
        // Show win UI
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowWinMessage();
        }
        
        // Pause game
        Time.timeScale = 0f;
    }
    
    public void LoseGame()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("GAME OVER - YOU LOSE!");
        
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
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