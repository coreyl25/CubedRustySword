using UnityEngine;

public class CollisionDemo : MonoBehaviour
{
    private PlayerHealth playerHealth;
    public int coinValue = 5; // Changed from 1 to 5 points per coin
    
    [Header("Sound Effects")]
    public AudioClip coinCollectSFX;
    public float coinCollectVolume = 0.8f;
    
    private AudioSource audioSource;
    
    void Start()
    {
        // Get reference to the PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found on player!");
        }
        
        // Initialize audio source
        InitializeAudioSource();
    }
    
    void InitializeAudioSource()
    {
        // Create audio source for coin collection sound
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = coinCollectVolume;
        audioSource.spatialBlend = 0f; // 2D sound
        
        Debug.Log("CollisionDemo audio source initialized");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check what the player collided with using GameObject names
        if (collision.gameObject.name == "Wall")
        {
            Debug.Log("Ouch!!!");
            // Walls cause damage
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
        else if (collision.gameObject.name == "TreasureCube")
        {
            Debug.Log("You found the treasure!");
        }
        else if (collision.gameObject.name.Contains("Coin"))
        {
            Debug.Log("You collected a coin worth " + coinValue + " points!");
            
            // Play coin collection sound
            PlayCoinCollectSound();
            
            // Add score
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddScore(coinValue);
            }
            
            // Notify GameManager that a coin was collected
            if (GameManager.instance != null)
            {
                GameManager.instance.CoinCollected();
            }
            
            // Destroy the coin
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.name == "Hazard" || collision.gameObject.name.Contains("Hazard"))
        {
            // Generic hazard - causes damage
            Debug.Log("Hit a hazard!");
            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
        
        // Optional: Keep the original debug message for other objects
        Debug.Log("Player collided with: " + collision.gameObject.name);
    }
    
    void PlayCoinCollectSound()
    {
        if (coinCollectSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(coinCollectSFX);
            Debug.Log("Playing coin collect sound");
        }
        else
        {
            Debug.LogWarning("Coin collect SFX not assigned or audio source missing!");
        }
    }
    
    void Update()
    {
    }
}