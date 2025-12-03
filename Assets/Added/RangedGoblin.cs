using UnityEngine;

public class RangedGoblin : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Vector3 pointA;
    public Vector3 pointB;
    public float patrolSpeed = 2f;
    
    [Header("Chase Settings")]
    public float chaseSpeed = 3.5f;
    public float chaseRange = 8f; // Longer range for ranged enemy
    
    [Header("Attack Settings")]
    public float attackRange = 6f; // Stop at this distance to shoot
    public GameObject projectilePrefab; // Optional - will be created if not assigned
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;
    public int damage = 1;
    public float knockbackForce = 15f;
    
    [Header("Colors")]
    public Color patrolColor = Color.white;
    public Color chaseColor = new Color(1f, 0.5f, 0f); // Orange
    
    [Header("Score")]
    public int scoreValue = 15;
    
    [Header("Sound Effects")]
    public AudioClip alertSFX;
    public AudioClip chaseSFX;
    public AudioClip projectileSFX;
    public float alertVolume = 0.7f;
    public float chaseVolume = 0.5f;
    public float projectileVolume = 0.6f;
    
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
    private Material projectileMaterial;
    
    // Audio sources
    private AudioSource alertAudioSource;
    private AudioSource chaseAudioSource;
    private AudioSource projectileAudioSource;
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
            
            Debug.Log("RangedGoblin found player: " + playerObject.name);
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
        
        // Create projectile prefab if not assigned
        if (projectilePrefab == null)
        {
            CreateProjectilePrefab();
        }
        
        // Create material for projectiles
        CreateProjectileMaterial();
        
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
        
        // Create audio source for projectile firing sound (one-shot)
        projectileAudioSource = gameObject.AddComponent<AudioSource>();
        projectileAudioSource.playOnAwake = false;
        projectileAudioSource.volume = projectileVolume;
        projectileAudioSource.spatialBlend = 1f; // 3D sound
        projectileAudioSource.minDistance = 5f;
        projectileAudioSource.maxDistance = 20f;
        
        Debug.Log("RangedGoblin audio sources initialized");
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
    
    void CreateProjectilePrefab()
    {
        Debug.Log("Creating projectile prefab programmatically...");
        
        // Create a sphere GameObject
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        // Add Rigidbody
        Rigidbody projRb = projectile.AddComponent<Rigidbody>();
        projRb.useGravity = false;
        projRb.isKinematic = true;
        
        // Add Projectile script
        projectile.AddComponent<Projectile>();
        
        // Store as prefab reference
        projectilePrefab = projectile;
        
        // Deactivate it (we'll instantiate copies)
        projectile.SetActive(false);
        
        Debug.Log("Projectile prefab created successfully!");
    }
    
    void CreateProjectileMaterial()
    {
        // Create a red material for projectiles
        projectileMaterial = new Material(Shader.Find("Standard"));
        projectileMaterial.color = Color.red;
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
                
                ChasePlayer(distanceToPlayer);
                
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
    
    void ChasePlayer(float distanceToPlayer)
    {
        // Only move closer if outside attack range
        if (distanceToPlayer > attackRange)
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
        else
        {
            // Within attack range, just face the player
            Vector3 direction = (player.position - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
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
        
        // Face the player before shooting
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = lookRotation;
        }
        
        // Play projectile firing sound
        PlayProjectileSound();
        
        // Spawn and fire projectile
        if (projectilePrefab != null)
        {
            // Spawn projectile slightly in front of goblin
            Vector3 spawnPosition = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectile.SetActive(true);
            
            // Apply material
            Renderer projRend = projectile.GetComponent<Renderer>();
            if (projRend != null && projectileMaterial != null)
            {
                projRend.material = projectileMaterial;
            }
            
            // Get the Projectile component and initialize it
            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                Vector3 shootDirection = (player.position - transform.position).normalized;
                projectileScript.Initialize(shootDirection, projectileSpeed, damage, knockbackForce, chaseRange, playerObject);
            }
            else
            {
                Debug.LogError("Projectile prefab is missing Projectile script!");
            }
            
            Debug.Log("Ranged Goblin fired projectile!");
        }
        
        // Set attack cooldown
        canAttack = false;
        attackTimer = 0f;
        
        // Return to chase state
        ChangeState(GoblinState.Chase);
    }
    
    void PlayProjectileSound()
    {
        if (projectileSFX != null && projectileAudioSource != null)
        {
            projectileAudioSource.PlayOneShot(projectileSFX);
            Debug.Log("RangedGoblin played projectile firing sound!");
        }
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
                Debug.Log("RangedGoblin played alert sound!");
            }
            
            // Start looping chase sound
            if (chaseSFX != null && chaseAudioSource != null && !chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Play();
                Debug.Log("RangedGoblin started chase sound loop");
            }
        }
        
        // Returning to patrol from chase/attack
        if (newState == GoblinState.Patrol && (oldState == GoblinState.Chase || oldState == GoblinState.Attack))
        {
            // Stop chase sound
            if (chaseAudioSource != null && chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Stop();
                Debug.Log("RangedGoblin stopped chase sound");
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
        Debug.Log("Ranged Goblin defeated!");
        
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