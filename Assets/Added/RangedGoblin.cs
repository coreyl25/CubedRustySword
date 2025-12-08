using UnityEngine;

public class RangedGoblin : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Vector3 pointA;
    public Vector3 pointB;
    public float patrolSpeed = 2f;
    
    [Header("Edge Detection - SAFETY BACKUP")]
    [Tooltip("Emergency edge detection - only triggers if goblin goes beyond patrol points")]
    public float emergencyEdgeCheckDistance = 1.0f;
    public float groundCheckDepth = 2.0f;
    
    [Header("Chase Settings")]
    public float chaseSpeed = 3.5f;
    public float chaseRange = 8f;
    
    [Header("Attack Settings")]
    public float attackRange = 6f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;
    public int damage = 1;
    public float knockbackForce = 15f;
    
    [Header("Colors")]
    public Color patrolColor = Color.white;
    public Color chaseColor = new Color(1f, 0.5f, 0f);
    
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
    public GameObject playerObject;
    
    private enum GoblinState { Patrol, Chase, Attack }
    private GoblinState currentState = GoblinState.Patrol;
    
    private Vector3 currentTarget;
    private Renderer rend;
    private Transform player;
    private Rigidbody rb;
    private bool canAttack = true;
    private float attackTimer = 0f;
    private Vector3 startPosition;
    private PlayerHealth playerHealth;
    private Material projectileMaterial;
    
    private AudioSource alertAudioSource;
    private AudioSource chaseAudioSource;
    private AudioSource projectileAudioSource;
    private bool hasPlayedAlert = false;
    
    void Start()
    {
        startPosition = transform.position;
        
        // Start moving toward the closest point
        float distToA = Vector3.Distance(transform.position, pointA);
        float distToB = Vector3.Distance(transform.position, pointB);
        currentTarget = (distToA < distToB) ? pointA : pointB;
        
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        
        if (playerObject == null)
        {
            playerObject = FindPlayerObject();
        }
        
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
        
        if (rend != null)
        {
            rend.material.color = patrolColor;
        }
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        if (projectilePrefab == null)
        {
            CreateProjectilePrefab();
        }
        
        CreateProjectileMaterial();
        InitializeAudioSources();
        
        float patrolDistance = Vector3.Distance(pointA, pointB);
        Debug.Log($"[RangedGoblin] {gameObject.name} initialized - Patrol distance: {patrolDistance:F2} units");
        Debug.Log($"[RangedGoblin] Will continuously patrol between Point A and Point B");
    }
    
    void InitializeAudioSources()
    {
        alertAudioSource = gameObject.AddComponent<AudioSource>();
        alertAudioSource.playOnAwake = false;
        alertAudioSource.volume = alertVolume;
        alertAudioSource.spatialBlend = 1f;
        alertAudioSource.minDistance = 5f;
        alertAudioSource.maxDistance = 20f;
        
        chaseAudioSource = gameObject.AddComponent<AudioSource>();
        chaseAudioSource.playOnAwake = false;
        chaseAudioSource.loop = true;
        chaseAudioSource.volume = chaseVolume;
        chaseAudioSource.spatialBlend = 1f;
        chaseAudioSource.minDistance = 5f;
        chaseAudioSource.maxDistance = 20f;
        
        if (chaseSFX != null)
        {
            chaseAudioSource.clip = chaseSFX;
        }
        
        projectileAudioSource = gameObject.AddComponent<AudioSource>();
        projectileAudioSource.playOnAwake = false;
        projectileAudioSource.volume = projectileVolume;
        projectileAudioSource.spatialBlend = 1f;
        projectileAudioSource.minDistance = 5f;
        projectileAudioSource.maxDistance = 20f;
        
        Debug.Log("RangedGoblin audio sources initialized");
    }
    
    GameObject FindPlayerObject()
    {
        PlayerHealth[] allPlayerHealths = FindObjectsOfType<PlayerHealth>();
        if (allPlayerHealths.Length > 0)
        {
            return allPlayerHealths[0].gameObject;
        }
        
        PlayerPhysics playerPhysics = FindObjectOfType<PlayerPhysics>();
        if (playerPhysics != null)
        {
            return playerPhysics.gameObject;
        }
        
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
        
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        
        Rigidbody projRb = projectile.AddComponent<Rigidbody>();
        projRb.useGravity = false;
        projRb.isKinematic = true;
        
        projectile.AddComponent<Projectile>();
        projectilePrefab = projectile;
        projectile.SetActive(false);
        
        Debug.Log("Projectile prefab created successfully!");
    }
    
    void CreateProjectileMaterial()
    {
        projectileMaterial = new Material(Shader.Find("Standard"));
        projectileMaterial.color = Color.red;
    }
    
    void Update()
    {
        if (player == null) return;
        
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
        bool playerIsInvincible = (playerHealth != null && playerHealth.IsInvincible());
        
        switch (currentState)
        {
            case GoblinState.Patrol:
                Patrol();
                
                if (!playerIsInvincible && distanceToPlayer <= chaseRange)
                {
                    ChangeState(GoblinState.Chase);
                }
                break;
                
            case GoblinState.Chase:
                if (playerIsInvincible)
                {
                    ChangeState(GoblinState.Patrol);
                    break;
                }
                
                ChasePlayer(distanceToPlayer);
                
                if (distanceToPlayer <= attackRange && canAttack)
                {
                    ChangeState(GoblinState.Attack);
                }
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
        // Move toward current target point
        transform.position = Vector3.MoveTowards(
            transform.position, 
            currentTarget, 
            patrolSpeed * Time.deltaTime
        );
        
        // Check if reached the target point
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        
        if (distanceToTarget < 0.1f)
        {
            // Reached target - switch to the other point
            if (currentTarget == pointA)
            {
                currentTarget = pointB;
                Debug.Log($"[RangedGoblin] {gameObject.name} reached Point A, now heading to Point B");
            }
            else
            {
                currentTarget = pointA;
                Debug.Log($"[RangedGoblin] {gameObject.name} reached Point B, now heading to Point A");
            }
        }
        
        // Emergency edge detection (backup safety)
        float distanceFromA = Vector3.Distance(transform.position, pointA);
        float distanceFromB = Vector3.Distance(transform.position, pointB);
        float patrolPathLength = Vector3.Distance(pointA, pointB);
        
        // If goblin is further from both points than the patrol path length, something's wrong
        if (distanceFromA > patrolPathLength * 1.5f && distanceFromB > patrolPathLength * 1.5f)
        {
            Debug.LogWarning($"[RangedGoblin] {gameObject.name} went off patrol path! Resetting to closest point.");
            currentTarget = (distanceFromA < distanceFromB) ? pointA : pointB;
            transform.position = currentTarget;
        }
    }
    
    void ChasePlayer(float distanceToPlayer)
    {
        if (distanceToPlayer > attackRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(
                transform.position, 
                player.position, 
                chaseSpeed * Time.deltaTime
            );
            
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
        else
        {
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
        if (playerHealth != null && playerHealth.IsInvincible())
        {
            ChangeState(GoblinState.Patrol);
            return;
        }
        
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = lookRotation;
        }
        
        PlayProjectileSound();
        
        if (projectilePrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 1f + Vector3.up * 0.5f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectile.SetActive(true);
            
            Renderer projRend = projectile.GetComponent<Renderer>();
            if (projRend != null && projectileMaterial != null)
            {
                projRend.material = projectileMaterial;
            }
            
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
        
        canAttack = false;
        attackTimer = 0f;
        
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
        GoblinState oldState = currentState;
        currentState = newState;
        
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
        
        HandleStateAudio(oldState, newState);
    }
    
    void HandleStateAudio(GoblinState oldState, GoblinState newState)
    {
        if (oldState == GoblinState.Patrol && newState == GoblinState.Chase)
        {
            if (!hasPlayedAlert && alertSFX != null && alertAudioSource != null)
            {
                alertAudioSource.PlayOneShot(alertSFX);
                hasPlayedAlert = true;
                Debug.Log("RangedGoblin played alert sound!");
            }
            
            if (chaseSFX != null && chaseAudioSource != null && !chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Play();
                Debug.Log("RangedGoblin started chase sound loop");
            }
        }
        
        if (newState == GoblinState.Patrol && (oldState == GoblinState.Chase || oldState == GoblinState.Attack))
        {
            if (chaseAudioSource != null && chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Stop();
                Debug.Log("RangedGoblin stopped chase sound");
            }
            
            hasPlayedAlert = false;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        PlayerHealth hitPlayerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (hitPlayerHealth != null)
        {
            Vector3 hitDirection = collision.contacts[0].normal;
            
            if (hitDirection.y < -0.5f)
            {
                Die();
            }
        }
    }
    
    void Die()
    {
        Debug.Log("Ranged Goblin defeated!");
        
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
        
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreValue);
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw patrol points
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.5f);
        Gizmos.DrawLine(pointA, pointA + Vector3.up * 2f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pointB, 0.5f);
        Gizmos.DrawLine(pointB, pointB + Vector3.up * 2f);
        
        // Draw patrol path
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA, pointB);
        
        // Draw chase range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Draw attack range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Show current target during play mode
        if (Application.isPlaying && currentTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget);
            Gizmos.DrawWireSphere(currentTarget, 0.3f);
        }
    }
    
    void OnDestroy()
    {
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
    }
}