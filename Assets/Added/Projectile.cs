using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private int damage;
    private float knockbackForce;
    private float goblinChaseRange;
    private GameObject playerObject;
    private float lifetime = 4f;
    private float spawnTime;
    private bool hasHit = false;
    private bool isInitialized = false;
    
    void Start()
    {
        spawnTime = Time.time;
        
        // If not initialized by RangedGoblin, try to auto-initialize
        if (!isInitialized)
        {
            AutoInitialize();
        }
    }
    
    void AutoInitialize()
    {
        Debug.Log("Projectile auto-initializing...");
        
        // Find player
        playerObject = FindPlayerObject();
        
        if (playerObject != null)
        {
            // Set default values
            Vector3 toPlayer = (playerObject.transform.position - transform.position).normalized;
            Initialize(toPlayer, 10f, 1, 15f, 8f, playerObject);
        }
        else
        {
            Debug.LogWarning("Projectile could not find player for auto-initialization!");
        }
    }
    
    GameObject FindPlayerObject()
    {
        // Method 1: Find by PlayerHealth component
        PlayerHealth[] allPlayerHealths = FindObjectsOfType<PlayerHealth>();
        if (allPlayerHealths.Length > 0)
        {
            return allPlayerHealths[0].gameObject;
        }
        
        // Method 2: Find by PlayerPhysics component
        PlayerPhysics playerPhysics = FindObjectOfType<PlayerPhysics>();
        if (playerPhysics != null)
        {
            return playerPhysics.gameObject;
        }
        
        // Method 3: Find by name
        GameObject player = GameObject.Find("Player");
        if (player != null) return player;
        
        player = GameObject.Find("player");
        if (player != null) return player;
        
        player = GameObject.Find("Capsule");
        if (player != null) return player;
        
        return null;
    }
    
    public void Initialize(Vector3 shootDirection, float projectileSpeed, int projectileDamage, float knockback, float chaseRange, GameObject player)
    {
        direction = shootDirection.normalized;
        speed = projectileSpeed;
        damage = projectileDamage;
        knockbackForce = knockback;
        goblinChaseRange = chaseRange;
        playerObject = player;
        spawnTime = Time.time;
        isInitialized = true;
        
        Debug.Log("Projectile initialized with speed: " + speed);
    }
    
    void Update()
    {
        // Don't move if not initialized
        if (!isInitialized) return;
        
        // Move projectile forward
        transform.position += direction * speed * Time.deltaTime;
        
        // Check if lifetime expired (4 seconds)
        if (Time.time - spawnTime >= lifetime)
        {
            Debug.Log("Projectile despawned after 4 seconds");
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Prevent multiple hits
        if (hasHit) return;
        
        hasHit = true;
        
        // Check if hit object has PlayerHealth component (this identifies the player)
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (playerHealth != null)
        {
            Debug.Log("Projectile hit player!");
            
            // Deal damage if player is not invincible
            if (!playerHealth.IsInvincible())
            {
                playerHealth.TakeDamage();
                
                // Calculate knockback direction (away from projectile's direction)
                Vector3 knockbackDirection = direction;
                knockbackDirection.y = 0.3f; // Add slight upward component
                knockbackDirection.Normalize();
                
                // Apply stronger knockback to push player outside chase range
                Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    // Reset velocity first to ensure consistent knockback
                    playerRb.linearVelocity = Vector3.zero;
                    
                    // Calculate knockback force needed to push outside chase range
                    // Use extra force to ensure they go far enough
                    float extraForce = goblinChaseRange * 0.5f;
                    playerRb.AddForce(knockbackDirection * (knockbackForce + extraForce), ForceMode.Impulse);
                    
                    Debug.Log("Knockback applied to player with force: " + (knockbackForce + extraForce));
                }
            }
            else
            {
                Debug.Log("Player is invincible - no damage dealt");
            }
            
            // Destroy projectile on impact with player
            Destroy(gameObject);
        }
        else
        {
            // Hit something else (wall, floor, etc.)
            Debug.Log("Projectile hit: " + collision.gameObject.name);
            Destroy(gameObject);
        }
    }
}