using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    private Vector3 startPosition;
    
    void Start()
    {
        // Store the starting position for respawning
        startPosition = transform.position;
        
        // Initialize lives
        currentLives = maxLives;
        
        // Update UI
        UpdateLivesUI();
    }
    
    public void TakeDamage()
    {
        if (currentLives <= 0)
            return; // Already dead, ignore further damage
        
        currentLives--;
        Debug.Log("Lives remaining: " + currentLives);
        
        UpdateLivesUI();
        
        if (currentLives > 0)
        {
            // Still have lives - respawn
            Respawn();
        }
        else
        {
            // No lives left - game over
            GameOver();
        }
    }
    
    void Respawn()
    {
        // Reset player position to start
        transform.position = startPosition;
        
        // Reset velocity if using Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("Player respawned!");
    }
    
    void GameOver()
    {
        Debug.Log("GAME OVER!");
        
        // Call GameManager to handle game over
        if (GameManager.instance != null)
        {
            GameManager.instance.LoseGame();
        }
        
        // Disable player controls
        PlayerPhysics playerPhysics = GetComponent<PlayerPhysics>();
        if (playerPhysics != null)
        {
            playerPhysics.enabled = false;
        }
    }
    
    void UpdateLivesUI()
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateLives(currentLives);
        }
    }
    
    // Optional: Method to add lives (for power-ups, etc.)
    public void AddLife()
    {
        if (currentLives < maxLives)
        {
            currentLives++;
            UpdateLivesUI();
            Debug.Log("Extra life! Lives: " + currentLives);
        }
    }
    
    public int GetCurrentLives()
    {
        return currentLives;
    }
}
