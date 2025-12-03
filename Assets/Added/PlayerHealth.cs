using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;
    private Vector3 startPosition;
    
    [Header("Invincibility Settings")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private Renderer playerRenderer;
    
    void Start()
    {
        // Store the starting position for respawning
        startPosition = transform.position;
        
        // Initialize lives
        currentLives = maxLives;
        
        // Get renderer for visual feedback
        playerRenderer = GetComponent<Renderer>();
        
        // Update UI
        UpdateLivesUI();
    }
    
    void Update()
    {
        // Don't update invincibility when game is paused
        if (Time.timeScale == 0f) return;
        
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer += Time.deltaTime;
            
            // Flash effect during invincibility
            if (playerRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                Color color = playerRenderer.material.color;
                color.a = alpha * 0.5f + 0.5f; // Flicker between 50% and 100% opacity
                playerRenderer.material.color = color;
            }
            
            // End invincibility
            if (invincibilityTimer >= invincibilityDuration)
            {
                isInvincible = false;
                invincibilityTimer = 0f;
                
                // Restore full opacity
                if (playerRenderer != null)
                {
                    Color color = playerRenderer.material.color;
                    color.a = 1f;
                    playerRenderer.material.color = color;
                }
            }
        }
    }
    
    public void TakeDamage()
    {
        // Don't take damage if invincible
        if (isInvincible)
        {
            Debug.Log("Player is invincible!");
            return;
        }
        
        if (currentLives <= 0)
            return; // Already dead, ignore further damage
        
        currentLives--;
        Debug.Log("Lives remaining: " + currentLives);
        
        // Activate invincibility
        isInvincible = true;
        invincibilityTimer = 0f;
        
        UpdateLivesUI();
        
        if (currentLives > 0)
        {
            // Still have lives - just got hit
            Debug.Log("Player hurt! Invincible for " + invincibilityDuration + " seconds.");
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
        
        // End invincibility
        isInvincible = false;
        
        // Restore full opacity
        if (playerRenderer != null)
        {
            Color color = playerRenderer.material.color;
            color.a = 1f;
            playerRenderer.material.color = color;
        }
        
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
    
    public bool IsInvincible()
    {
        return isInvincible;
    }
}