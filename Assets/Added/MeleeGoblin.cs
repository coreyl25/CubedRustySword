using UnityEngine;

public class MeleeGoblin : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Vector3 pointA;
    public Vector3 pointB;
    public float patrolSpeed = 2f;
    
    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float chaseRange = 5f;
    
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public int damage = 1;
    public float knockbackForce = 10f;
    public float attackCooldown = 1f;
    
    [Header("Colors")]
    public Color patrolColor = Color.green;
    public Color chaseColor = Color.red;
    
    [Header("Score")]
    public int scoreValue = 10;
    
    [Header("Sound Effects")]
    public AudioClip alertSFX;
    public AudioClip chaseSFX;
    public float alertVolume = 0.7f;
    public float chaseVolume = 0.5f;
    
    [Header("Player Reference (Optional - Auto-detected)")]
    public GameObject playerObject; // Optional - will auto-find if not assigned
    
    private enum GoblinState { Patrol, Chase, Attack }
    private GoblinState currentState = GoblinState.Patrol;
    
    private Vector3 patrolTarget;
    private Renderer rend;
    private Transform player;
    private Rigidbody rb;
    private bool canAttack = true;
    private float attackTimer = 0f;
    private Vector3 startPosition;
    private PlayerHealth playerHealth;
    
    // Audio sources
    private AudioSource alertAudioSource;
    private AudioSource chaseAudioSource;
    private bool hasPlayedAlert = false; // Track if alert has been played this chase sequence
    
    void Start()
    {
        // Initialize patrol target
        startPosition = transform.position;
        patrolTarget = pointB;
        
        // Get components
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        
        // Auto-find player if not assigned
        if (playerObject == null)
        {
            playerObject = FindPlayerObject();
        }
        
        // Get player reference
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
            
            if (playerHealth == null)
            {
                Debug.LogWarning("PlayerHealth component not found on player!");
            }
            
            Debug.Log("MeleeGoblin found player: " + playerObject.name);
        }
        else
        {
            Debug.LogError("Could not find player object! Make sure player has PlayerHealth component.");
        }
        
        // Set initial color
        if (rend != null)
        {
            rend.material.color = patrolColor;
        }
        
        // Configure Rigidbody if present
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        // Initialize audio sources
        InitializeAudioSources();
    }
    
    void InitializeAudioSources()
    {
        // Create audio source for alert sound (one-shot)
        alertAudioSource = gameObject.AddComponent<AudioSource>();
        alertAudioSource.playOnAwake = false;
        alertAudioSource.volume = alertVolume;
        alertAudioSource.spatialBlend = 1f; // 3D sound
        alertAudioSource.minDistance = 5f;
        alertAudioSource.maxDistance = 20f;
        
        // Create audio source for chase sound (looping)
        chaseAudioSource = gameObject.AddComponent<AudioSource>();
        chaseAudioSource.playOnAwake = false;
        chaseAudioSource.loop = true;
        chaseAudioSource.volume = chaseVolume;
        chaseAudioSource.spatialBlend = 1f; // 3D sound
        chaseAudioSource.minDistance = 5f;
        chaseAudioSource.maxDistance = 20f;
        
        if (chaseSFX != null)
        {
            chaseAudioSource.clip = chaseSFX;
        }
        
        Debug.Log("MeleeGoblin audio sources initialized");
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
        
        // Method 3: Find by name (common player names)
        GameObject player = GameObject.Find("Player");
        if (player != null) return player;
        
        player = GameObject.Find("player");
        if (player != null) return player;
        
        player = GameObject.Find("Capsule");
        if (player != null && player.GetComponent<PlayerHealth>() != null) return player;
        
        return null;
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Update attack cooldown
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
            }
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if player is invincible - if so, ignore them
        bool playerIsInvincible = (playerHealth != null && playerHealth.IsInvincible());
        
        // State machine
        switch (currentState)
        {
            case GoblinState.Patrol:
                Patrol();
                
                // Only chase if player is NOT invincible and is in range
                if (!playerIsInvincible && distanceToPlayer <= chaseRange)
                {
                    ChangeState(GoblinState.Chase);
                }
                break;
                
            case GoblinState.Chase:
                // If player becomes invincible, return to patrol
                if (playerIsInvincible)
                {
                    ChangeState(GoblinState.Patrol);
                    break;
                }
                
                ChasePlayer();
                
                // Check if player is in attack range
                if (distanceToPlayer <= attackRange && canAttack)
                {
                    ChangeState(GoblinState.Attack);
                }
                // Check if player escaped chase range
                else if (distanceToPlayer > chaseRange)
                {
                    ChangeState(GoblinState.Patrol);
                }
                break;
                
            case GoblinState.Attack:
                AttackPlayer();
                break;
        }
    }
    
    void Patrol()
    {
        // Move toward patrol target
        transform.position = Vector3.MoveTowards(
            transform.position, 
            patrolTarget, 
            patrolSpeed * Time.deltaTime
        );
        
        // Switch patrol target when reached
        if (Vector3.Distance(transform.position, patrolTarget) < 0.1f)
        {
            patrolTarget = (patrolTarget == pointA) ? pointB : pointA;
        }
    }
    
    void ChasePlayer()
    {
        // Move toward player
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(
            transform.position, 
            player.position, 
            chaseSpeed * Time.deltaTime
        );
        
        // Face the player
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
    
    void AttackPlayer()
    {
        // Don't attack if player is invincible
        if (playerHealth != null && playerHealth.IsInvincible())
        {
            ChangeState(GoblinState.Patrol);
            return;
        }
        
        // Try to damage player
        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
            
            // Knockback player - apply stronger force
            Vector3 knockbackDirection = (player.position - transform.position).normalized;
            
            // Make sure knockback goes slightly upward too
            knockbackDirection.y = 0.3f;
            knockbackDirection.Normalize();
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Reset velocity first to ensure consistent knockback
                playerRb.linearVelocity = Vector3.zero;
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
            
            Debug.Log("Goblin attacked player! Knockback applied.");
        }
        
        // Set attack cooldown
        canAttack = false;
        attackTimer = 0f;
        
        // Return to patrol state immediately
        ChangeState(GoblinState.Patrol);
    }
    
    void ChangeState(GoblinState newState)
    {
        // Store old state for comparison
        GoblinState oldState = currentState;
        currentState = newState;
        
        // Update color based on state
        if (rend != null)
        {
            switch (currentState)
            {
                case GoblinState.Patrol:
                    rend.material.color = patrolColor;
                    break;
                case GoblinState.Chase:
                case GoblinState.Attack:
                    rend.material.color = chaseColor;
                    break;
            }
        }
        
        // Handle audio based on state changes
        HandleStateAudio(oldState, newState);
    }
    
    void HandleStateAudio(GoblinState oldState, GoblinState newState)
    {
        // Entering chase state from patrol
        if (oldState == GoblinState.Patrol && newState == GoblinState.Chase)
        {
            // Play alert sound once
            if (!hasPlayedAlert && alertSFX != null && alertAudioSource != null)
            {
                alertAudioSource.PlayOneShot(alertSFX);
                hasPlayedAlert = true;
                Debug.Log("MeleeGoblin played alert sound!");
            }
            
            // Start looping chase sound
            if (chaseSFX != null && chaseAudioSource != null && !chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Play();
                Debug.Log("MeleeGoblin started chase sound loop");
            }
        }
        
        // Returning to patrol from chase/attack
        if (newState == GoblinState.Patrol && (oldState == GoblinState.Chase || oldState == GoblinState.Attack))
        {
            // Stop chase sound
            if (chaseAudioSource != null && chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Stop();
                Debug.Log("MeleeGoblin stopped chase sound");
            }
            
            // Reset alert flag so it can play again next chase
            hasPlayedAlert = false;
        }
        
        // Continue chase sound during attack state
        if (newState == GoblinState.Attack && oldState == GoblinState.Chase)
        {
            // Keep chase sound playing during attack
            // Don't stop it
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Check if it's the player by checking for PlayerHealth component
        PlayerHealth hitPlayerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (hitPlayerHealth != null)
        {
            // Check if collision is from above (player jumped on head)
            Vector3 hitDirection = collision.contacts[0].normal;
            
            // If the hit normal points downward, player hit from above
            if (hitDirection.y < -0.5f)
            {
                Die();
            }
        }
    }
    
    void Die()
    {
        Debug.Log("Goblin defeated!");
        
        // Stop all sounds
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
        
        // Add score
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreValue);
        }
        
        // Destroy goblin
        Destroy(gameObject);
    }
    
    // Visualize detection ranges in editor
    void OnDrawGizmosSelected()
    {
        // Draw patrol points
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.3f);
        Gizmos.DrawWireSphere(pointB, 0.3f);
        Gizmos.DrawLine(pointA, pointB);
        
        // Draw chase range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    void OnDestroy()
    {
        // Clean up audio when destroyed
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
    }
}