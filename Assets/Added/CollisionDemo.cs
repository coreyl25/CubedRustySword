using UnityEngine;

public class CollisionDemo : MonoBehaviour
{
    private PlayerHealth playerHealth;
    public int coinValue = 1; // Points per coin
    
    void Start()
    {
        // Get reference to the PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();
        
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found on player!");
        }
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
        else if (collision.gameObject.name == "Coin" || collision.gameObject.tag == "Coin")
        {
            Debug.Log("You collected a coin!");
            
            // Add score
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddScore(coinValue);
            }
            
            // Destroy the coin
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.name == "Hazard" || collision.gameObject.tag == "Hazard")
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
    
    void Update()
    {
    }
}