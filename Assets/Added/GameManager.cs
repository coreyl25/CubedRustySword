using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public int totalCoins;
    
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
        // Count all coins in the scene by name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int coinCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Coin"))
            {
                coinCount++;
            }
        }
        
        totalCoins = coinCount;
        Debug.Log("Total coins in level: " + totalCoins);
    }
    
    public void CheckWinCondition(int currentScore)
    {
        // Check if player collected all coins
        if (currentScore >= totalCoins)
        {
            WinGame();
        }
    }
    
    void WinGame()
    {
        Debug.Log("You Win!");
        
        // Show win UI
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowWinMessage();
        }
        
        // Disable player controls
        DisablePlayerControls();
    }
    
    public void LoseGame()
    {
        Debug.Log("Game Over!");
        
        // Show game over UI
        if (UIManager.instance != null)
        {
            UIManager.instance.ShowGameOverMessage();
        }
        
        // Disable player controls
        DisablePlayerControls();
    }
    
    void DisablePlayerControls()
    {
        PlayerPhysics playerPhysics = FindFirstObjectByType<PlayerPhysics>();
        if (playerPhysics != null)
        {
            playerPhysics.enabled = false;
        }
    }
}