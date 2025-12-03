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
    
    [Header("Jump Kill Settings")]
    public float jumpKillThreshold = 0.3f; // How much player must be above goblin (Y position difference)
    public float jumpKillVelocityThreshold = -1f; // Player must be falling (negative Y velocity)
    
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
    private bool hasPlayedAlert = false;
    
    // Jump kill tracking
    private bool isDead = false;
    
    void Start()
    {
        startPosition = transform.position;
        patrolTarget = pointB;
        
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
        DiagnoseAudio();
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
    
    void DiagnoseAudio()
    {
        Debug.Log("=== MELEE GOBLIN AUDIO DIAGNOSTIC (" + gameObject.name + ") ===");
        
        if (alertSFX != null)
        {
            Debug.Log("[✓] Alert SFX assigned: " + alertSFX.name);
        }
        else
        {
            Debug.LogWarning("[✗] Alert SFX NOT ASSIGNED - Please assign AlertSFX.wav in Inspector!");
        }
        
        if (chaseSFX != null)
        {
            Debug.Log("[✓] Chase SFX assigned: " + chaseSFX.name);
            chaseAudioSource.clip = chaseSFX;
        }
        else
        {
            Debug.LogWarning("[✗] Chase SFX NOT ASSIGNED - Please assign ChaseSFX1.wav in Inspector!");
        }
        
        Debug.Log("=================================================");
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
        transform.position = Vector3.MoveTowards(
            transform.position, 
            patrolTarget, 
            patrolSpeed * Time.deltaTime
        );
        
        if (Vector3.Distance(transform.position, patrolTarget) < 0.1f)
        {
            patrolTarget = (patrolTarget == pointA) ? pointB : pointA;
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
            else if (alertSFX == null)
            {
                Debug.LogWarning("[MeleeGoblin] Cannot play alert - AlertSFX.wav not assigned to " + gameObject.name);
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
            else if (chaseSFX == null)
            {
                Debug.LogWarning("[MeleeGoblin] Cannot play chase - ChaseSFX1.wav not assigned to " + gameObject.name);
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
        
        // Check if it's the player
        PlayerHealth hitPlayerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        
        if (hitPlayerHealth != null)
        {
            // Get player rigidbody to check velocity
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            
            // Calculate relative positions
            float playerY = collision.transform.position.y;
            float goblinY = transform.position.y;
            float yDifference = playerY - goblinY;
            
            // Check if player is falling (negative Y velocity)
            bool playerIsFalling = false;
            if (playerRb != null)
            {
                playerIsFalling = playerRb.linearVelocity.y < jumpKillVelocityThreshold;
            }
            
            // Check collision from above using contact points
            bool hitFromAbove = false;
            foreach (ContactPoint contact in collision.contacts)
            {
                // If contact normal points downward (from goblin's perspective), player hit from above
                if (contact.normal.y < -0.5f)
                {
                    hitFromAbove = true;
                    break;
                }
            }
            
            // Debug information
            Debug.Log($"[MeleeGoblin] Collision - Y Diff: {yDifference:F2}, Player Falling: {playerIsFalling}, Hit From Above: {hitFromAbove}");
            
            // Player successfully jumped on goblin's head
            if (hitFromAbove && playerIsFalling && yDifference > jumpKillThreshold)
            {
                Debug.Log("[MeleeGoblin] Player jumped on head - Goblin defeated!");
                
                // Bounce player upward slightly
                if (playerRb != null)
                {
                    Vector3 bounceVelocity = playerRb.linearVelocity;
                    bounceVelocity.y = 5f; // Small bounce
                    playerRb.linearVelocity = bounceVelocity;
                }
                
                Die();
            }
            else
            {
                // Player hit goblin from side or while not falling - goblin can damage player
                Debug.Log("[MeleeGoblin] Player hit from side/front - No jump kill");
                // Normal collision - let the attack system handle it
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pointA, 0.3f);
        Gizmos.DrawWireSphere(pointB, 0.3f);
        Gizmos.DrawLine(pointA, pointB);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Visualize jump kill zone (above the goblin)
        Gizmos.color = Color.cyan;
        Vector3 killZoneCenter = transform.position + Vector3.up * (jumpKillThreshold + 0.5f);
        Gizmos.DrawWireCube(killZoneCenter, new Vector3(1f, 0.2f, 1f));
    }
    
    void OnDestroy()
    {
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
    }
}