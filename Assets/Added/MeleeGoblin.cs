using UnityEngine;

public class MeleeGoblin : MonoBehaviour
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
    public float chaseSpeed = 4f;
    public float chaseRange = 5f;
    
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public int damage = 1;
    public float knockbackForce = 10f;
    public float attackCooldown = 1f;
    
    [Header("Jump Kill Settings")]
    public float jumpKillThreshold = 0.3f;
    public float jumpKillVelocityThreshold = -1f;
    
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
    
    private AudioSource alertAudioSource;
    private AudioSource chaseAudioSource;
    private bool hasPlayedAlert = false;
    
    private bool isDead = false;
    
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
        }
        else
        {
            Debug.LogError("Could not find player object!");
        }
        
        if (rend != null)
        {
            rend.material.color = patrolColor;
        }
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        InitializeAudioSources();
        
        float patrolDistance = Vector3.Distance(pointA, pointB);
        Debug.Log($"[MeleeGoblin] {gameObject.name} initialized - Patrol distance: {patrolDistance:F2} units");
        Debug.Log($"[MeleeGoblin] Will continuously patrol between Point A and Point B");
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
        
        Debug.Log("[MeleeGoblin] Audio sources initialized on " + gameObject.name);
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
    
    void Update()
    {
        if (player == null || isDead) return;
        
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
                
                ChasePlayer();
                
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
                Debug.Log($"[MeleeGoblin] {gameObject.name} reached Point A, now heading to Point B");
            }
            else
            {
                currentTarget = pointA;
                Debug.Log($"[MeleeGoblin] {gameObject.name} reached Point B, now heading to Point A");
            }
        }
        
        // Emergency edge detection (backup safety)
        // Only checks if goblin somehow goes beyond patrol points
        float distanceFromA = Vector3.Distance(transform.position, pointA);
        float distanceFromB = Vector3.Distance(transform.position, pointB);
        float patrolPathLength = Vector3.Distance(pointA, pointB);
        
        // If goblin is further from both points than the patrol path length, something's wrong
        if (distanceFromA > patrolPathLength * 1.5f && distanceFromB > patrolPathLength * 1.5f)
        {
            Debug.LogWarning($"[MeleeGoblin] {gameObject.name} went off patrol path! Resetting to closest point.");
            // Teleport back to closest point
            currentTarget = (distanceFromA < distanceFromB) ? pointA : pointB;
            transform.position = currentTarget;
        }
    }
    
    void ChasePlayer()
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
    
    void AttackPlayer()
    {
        if (playerHealth != null && playerHealth.IsInvincible())
        {
            ChangeState(GoblinState.Patrol);
            return;
        }
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
            
            Vector3 knockbackDirection = (player.position - transform.position).normalized;
            knockbackDirection.y = 0.3f;
            knockbackDirection.Normalize();
            
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
        
        canAttack = false;
        attackTimer = 0f;
        
        ChangeState(GoblinState.Patrol);
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
                Debug.Log("[MeleeGoblin] Played alert sound on " + gameObject.name);
            }
            
            if (chaseSFX != null && chaseAudioSource != null && !chaseAudioSource.isPlaying)
            {
                if (chaseAudioSource.clip == null)
                {
                    chaseAudioSource.clip = chaseSFX;
                }
                chaseAudioSource.Play();
                Debug.Log("[MeleeGoblin] Started chase sound on " + gameObject.name);
            }
        }
        
        if (newState == GoblinState.Patrol && (oldState == GoblinState.Chase || oldState == GoblinState.Attack))
        {
            if (chaseAudioSource != null && chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Stop();
                Debug.Log("[MeleeGoblin] Stopped chase sound on " + gameObject.name);
            }
            
            hasPlayedAlert = false;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        
        PlayerHealth hitPlayerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (hitPlayerHealth != null)
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            
            float playerY = collision.transform.position.y;
            float goblinY = transform.position.y;
            float yDifference = playerY - goblinY;
            
            bool playerIsFalling = false;
            if (playerRb != null)
            {
                playerIsFalling = playerRb.linearVelocity.y < jumpKillVelocityThreshold;
            }
            
            bool hitFromAbove = false;
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    hitFromAbove = true;
                    break;
                }
            }
            
            Debug.Log($"[MeleeGoblin] Collision - Y Diff: {yDifference:F2}, Player Falling: {playerIsFalling}, Hit From Above: {hitFromAbove}");
            
            if (hitFromAbove && playerIsFalling && yDifference > jumpKillThreshold)
            {
                Debug.Log("[MeleeGoblin] Player jumped on head - Goblin defeated!");
                
                if (playerRb != null)
                {
                    Vector3 bounceVelocity = playerRb.linearVelocity;
                    bounceVelocity.y = 5f;
                    playerRb.linearVelocity = bounceVelocity;
                }
                
                Die();
            }
            else
            {
                Debug.Log("[MeleeGoblin] Player hit from side/front - No jump kill");
            }
        }
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log("[MeleeGoblin] Goblin defeated!");
        
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
        
        // Show patrol distance
        float distance = Vector3.Distance(pointA, pointB);
        Vector3 midPoint = (pointA + pointB) / 2f;
        
        // Draw chase range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Draw attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
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