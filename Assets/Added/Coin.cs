using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    public int coinValue = 5; // Points awarded when collected
    
    [Header("Sound Effects")]
    public AudioClip collectSound;
    public float collectVolume = 0.8f;
    
    [Header("Visual Effects (Optional)")]
    public bool rotateConstantly = true;
    public float rotationSpeed = 100f;
    
    private bool hasBeenCollected = false;
    
    void Update()
    {
        // Optional: Make coin rotate for visual appeal
        if (rotateConstantly)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Prevent double collection
        if (hasBeenCollected) return;
        
        // Check if it's the player
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            CollectCoin();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Prevent double collection
        if (hasBeenCollected) return;
        
        // Check if it's the player
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            CollectCoin();
        }
    }
    
    void CollectCoin()
    {
        hasBeenCollected = true;
        
        Debug.Log("=== COIN COLLECTED ===");
        Debug.Log("Coin: " + gameObject.name);
        Debug.Log("Value: " + coinValue + " points");
        
        // Play collection sound
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position, collectVolume);
        }
        
        // Add score
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(coinValue);
            Debug.Log("Score added to ScoreManager");
        }
        else
        {
            Debug.LogError("ScoreManager.instance is NULL!");
        }
        
        // Notify GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.CoinCollected();
            Debug.Log("GameManager notified of coin collection");
        }
        else
        {
            Debug.LogError("GameManager.instance is NULL!");
        }
        
        Debug.Log("=====================");
        
        // Destroy the coin
        Destroy(gameObject);
    }
}
