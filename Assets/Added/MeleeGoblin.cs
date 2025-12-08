using UnityEngine;

public class MeleeGoblin : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Vector3 pointA;
    public Vector3 pointB;
    public float patrolSpeed = 2f;
    public float waypointReachedDistance = 0.3f; // Increased threshold for reaching waypoints
    
    [Header("Chase Settings")]
    public float chaseSpeed = 4f;
    public float chaseRange = 5f;
    
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public int damage = 1;
    public float knockbackForce = 10f;
    public float attackCooldown = 1f;
    
    [Header("Jump Kill Settings")]
    [Tooltip("Minimum height difference for jump kill (player bottom - goblin top)")]
    public float jumpKillHeightThreshold = 0.2f;
    [Tooltip("Maximum downward velocity required for jump kill (negative value)")]
    public float jumpKillVelocityThreshold = -1f;
    [Tooltip("How much the player bounces after jump kill")]
    public float jumpKillBounceForce = 8f;
    [Tooltip("If true, uses contact point detection. If false, uses simple height comparison")]
    public bool useContactPointDetection = false;
    
    [Header("Colors")]
    public Color patrolColor = Color.green;
    public Color chaseColor = Color.red;
    
    [Header("Score")]
    public int scoreValue = 10;
    
    [Header("Sound Effects")]
    public AudioClip alertSFX;
    public AudioClip chaseSFX;
    public AudioClip deathSFX;
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
    private PlayerHealth playerHealth;
    
    private AudioSource alertAudioSource;
    private AudioSource chaseAudioSource;
    private bool hasPlayedAlert = false;
    
    private bool isDead = false;
    private bool isMovingToB = true; // Track which direction we're patrolling
    private bool playerJustKilledGoblin = false; // Prevent damage during jump kill
    
    void Start()
    {
        // Initialize patrol direction
        float distToA = Vector3.Distance(transform.position, pointA);
        float distToB = Vector3.Distance(transform.position, pointB);
        
        if (distToA < distToB)
        {
            currentTarget = pointB;
            isMovingToB = true;
        }
        else
        {
            currentTarget = pointA;
            isMovingToB = false;
        }
        
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();
        
        // Find player
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
                Debug.LogWarning("[MeleeGoblin] PlayerHealth component not found on player!");
            }
        }
        else
        {
            Debug.LogError("[MeleeGoblin] Could not find player object!");
        }
        
        // Set initial color
        if (rend != null)
        {
            rend.material.color = patrolColor;
        }
        
        // Configure Rigidbody
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = false; // Ensure physics works properly
        }
        
        InitializeAudioSources();
        
        Debug.Log($"[MeleeGoblin] {gameObject.name} initialized");
        Debug.Log($"[MeleeGoblin] Starting patrol from {(isMovingToB ? "A to B" : "B to A")}");
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
        
        GameObject p = GameObject.Find("Player");
        if (p != null) return p;
        
        p = GameObject.Find("Capsule");
        if (p != null && p.GetComponent<PlayerHealth>() != null) return p;
        
        return null;
    }
    
    void Update()
    {
        if (player == null || isDead) return;
        
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
        bool playerIsInvincible = (playerHealth != null && playerHealth.IsInvincible());
        
        // Don't attack if player just killed this goblin (prevents damage during jump kill)
        if (playerJustKilledGoblin) return;
        
        // State machine
        switch (currentState)
        {
            case GoblinState.Patrol:
                Patrol();
                
                // Transition to Chase if player is in range and not invincible
                if (!playerIsInvincible && distanceToPlayer <= chaseRange)
                {
                    ChangeState(GoblinState.Chase);
                }
                break;
                
            case GoblinState.Chase:
                // Return to patrol if player becomes invincible
                if (playerIsInvincible)
                {
                    ChangeState(GoblinState.Patrol);
                    break;
                }
                
                ChasePlayer();
                
                // Transition to Attack if in attack range
                if (distanceToPlayer <= attackRange && canAttack)
                {
                    ChangeState(GoblinState.Attack);
                }
                // Return to Patrol if player is too far
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
        // Calculate direction to current target
        Vector3 direction = (currentTarget - transform.position).normalized;
        
        // Move toward target using Rigidbody for consistent physics
        if (rb != null)
        {
            Vector3 moveVelocity = direction * patrolSpeed;
            moveVelocity.y = rb.linearVelocity.y; // Preserve vertical velocity
            rb.linearVelocity = moveVelocity;
        }
        else
        {
            // Fallback to transform movement
            transform.position = Vector3.MoveTowards(
                transform.position, 
                currentTarget, 
                patrolSpeed * Time.deltaTime
            );
        }
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        
        // Check if we've reached the waypoint
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget);
        
        if (distanceToTarget <= waypointReachedDistance)
        {
            // Switch to the other patrol point and continue patrolling
            if (isMovingToB)
            {
                currentTarget = pointA;
                isMovingToB = false;
                Debug.Log($"[MeleeGoblin] {gameObject.name} reached Point B, now heading back to Point A");
            }
            else
            {
                currentTarget = pointB;
                isMovingToB = true;
                Debug.Log($"[MeleeGoblin] {gameObject.name} reached Point A, now heading back to Point B");
            }
            
            // Immediately start moving toward the new target (no pause)
            direction = (currentTarget - transform.position).normalized;
            if (rb != null)
            {
                Vector3 moveVelocity = direction * patrolSpeed;
                moveVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = moveVelocity;
            }
        }
    }
    
    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        
        // Move toward player using Rigidbody
        if (rb != null)
        {
            Vector3 moveVelocity = direction * chaseSpeed;
            moveVelocity.y = rb.linearVelocity.y; // Preserve vertical velocity
            rb.linearVelocity = moveVelocity;
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                player.position, 
                chaseSpeed * Time.deltaTime
            );
        }
        
        // Rotate to face player
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }
    
    void AttackPlayer()
    {
        // Don't attack invincible player
        if (playerHealth != null && playerHealth.IsInvincible())
        {
            ChangeState(GoblinState.Patrol);
            return;
        }
        
        // Check if we should still attack (player might have moved)
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > attackRange)
        {
            // Player moved out of range
            ChangeState(GoblinState.Chase);
            return;
        }
        
        // NOTE: Damage is now handled in OnCollisionEnter to prevent
        // damage when player jumps on goblin's head
        // This method now just transitions states
        
        Debug.Log("[MeleeGoblin] Attack state - waiting for collision or range change");
        
        // Start attack cooldown
        canAttack = false;
        attackTimer = 0f;
        
        // Return to chase
        ChangeState(GoblinState.Chase);
    }
    
    void ChangeState(GoblinState newState)
    {
        GoblinState oldState = currentState;
        currentState = newState;
        
        // When returning to patrol from chase, resume at nearest patrol point
        if (newState == GoblinState.Patrol && oldState != GoblinState.Patrol)
        {
            // Find which patrol point is closer
            float distToA = Vector3.Distance(transform.position, pointA);
            float distToB = Vector3.Distance(transform.position, pointB);
            
            if (distToA < distToB)
            {
                currentTarget = pointB;
                isMovingToB = true;
                Debug.Log($"[MeleeGoblin] Resuming patrol - heading to Point B from current position");
            }
            else
            {
                currentTarget = pointA;
                isMovingToB = false;
                Debug.Log($"[MeleeGoblin] Resuming patrol - heading to Point A from current position");
            }
        }
        
        // Update visual feedback
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
        // Entering chase state
        if (oldState == GoblinState.Patrol && newState == GoblinState.Chase)
        {
            // Play alert sound once
            if (!hasPlayedAlert && alertSFX != null && alertAudioSource != null)
            {
                alertAudioSource.PlayOneShot(alertSFX);
                hasPlayedAlert = true;
            }
            
            // Start chase music loop
            if (chaseSFX != null && chaseAudioSource != null && !chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Play();
            }
        }
        
        // Leaving chase state
        if (newState == GoblinState.Patrol && (oldState == GoblinState.Chase || oldState == GoblinState.Attack))
        {
            // Stop chase music
            if (chaseAudioSource != null && chaseAudioSource.isPlaying)
            {
                chaseAudioSource.Stop();
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
            // FIRST: Check for jump kill BEFORE any damage is dealt
            // This ensures jump kill detection happens before attack logic
            
            // Get player's Rigidbody to check velocity
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            
            // Get the bounds of both objects to calculate actual height difference
            Bounds playerBounds = GetObjectBounds(collision.gameObject);
            Bounds goblinBounds = GetObjectBounds(gameObject);
            
            // Calculate the bottom of the player and top of the goblin
            float playerBottom = playerBounds.min.y;
            float goblinTop = goblinBounds.max.y;
            
            // Height difference (positive if player bottom is above goblin top)
            float heightDifference = playerBottom - goblinTop;
            
            // Check if player is falling downward
            bool playerIsFalling = false;
            float playerVerticalVelocity = 0f;
            
            if (playerRb != null)
            {
                playerVerticalVelocity = playerRb.linearVelocity.y;
                playerIsFalling = playerVerticalVelocity < jumpKillVelocityThreshold;
            }
            
            // Check collision contact points for top hit (optional method)
            bool hitFromTop = false;
            Vector3 averageContactPoint = Vector3.zero;
            
            if (collision.contacts.Length > 0)
            {
                foreach (ContactPoint contact in collision.contacts)
                {
                    averageContactPoint += contact.point;
                    
                    // If contact normal points downward, player hit from above
                    // We use a more lenient threshold for cubes
                    if (contact.normal.y < -0.1f)
                    {
                        hitFromTop = true;
                    }
                }
                averageContactPoint /= collision.contacts.Length;
                
                // Additional check: is the contact point above the goblin's center?
                if (averageContactPoint.y > transform.position.y)
                {
                    hitFromTop = true;
                }
            }
            
            // Debug information
            Debug.Log($"[MeleeGoblin] === COLLISION DETECTION ===");
            Debug.Log($"  Goblin State: {currentState}");
            Debug.Log($"  Player Bottom Y: {playerBottom:F2}");
            Debug.Log($"  Goblin Top Y: {goblinTop:F2}");
            Debug.Log($"  Height Diff (bottom-top): {heightDifference:F2} (threshold: {jumpKillHeightThreshold})");
            Debug.Log($"  Player Velocity Y: {playerVerticalVelocity:F2} (threshold: {jumpKillVelocityThreshold})");
            Debug.Log($"  Player Falling: {playerIsFalling}");
            Debug.Log($"  Hit From Top: {hitFromTop}");
            Debug.Log($"  Average Contact Y: {averageContactPoint.y:F2} vs Goblin Center Y: {transform.position.y:F2}");
            
            // Jump kill conditions:
            // METHOD 1: Simple height + velocity check (more reliable for cubes)
            bool simpleCheck = (heightDifference > -jumpKillHeightThreshold) && playerIsFalling;
            
            // METHOD 2: Contact point detection
            bool contactCheck = hitFromTop && playerIsFalling;
            
            // Use the selected method or combine both
            bool isJumpKill = useContactPointDetection ? contactCheck : simpleCheck;
            
            Debug.Log($"  Simple Check: {simpleCheck}");
            Debug.Log($"  Contact Check: {contactCheck}");
            Debug.Log($"  Final Result: {(isJumpKill ? "JUMP KILL!" : "Normal collision")}");
            Debug.Log($"================================");
            
            // CRITICAL: Process jump kill FIRST before any damage logic
            if (isJumpKill)
            {
                Debug.Log("[MeleeGoblin] ✓ JUMP KILL! Player defeated goblin!");
                
                // Set flag to prevent damage from being dealt
                playerJustKilledGoblin = true;
                
                // Bounce player upward
                if (playerRb != null)
                {
                    Vector3 bounceVelocity = playerRb.linearVelocity;
                    bounceVelocity.y = jumpKillBounceForce;
                    playerRb.linearVelocity = bounceVelocity;
                }
                
                // Kill the goblin immediately - this prevents any attack logic from running
                Die();
                
                // RETURN HERE - don't process any attack logic
                return;
            }
            
            // If we reach here, it's NOT a jump kill - goblin can attack
            Debug.Log("[MeleeGoblin] ✗ No jump kill - checking if goblin should attack");
            
            // Only deal damage if goblin is in attack range AND can attack
            // This prevents damage during normal movement/chasing
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer <= attackRange && canAttack && !hitPlayerHealth.IsInvincible())
            {
                Debug.Log("[MeleeGoblin] Goblin dealing damage to player (side/front collision)");
                
                hitPlayerHealth.TakeDamage();
                
                // Apply knockback
                Vector3 knockbackDirection = (collision.transform.position - transform.position).normalized;
                knockbackDirection.y = 0.3f;
                knockbackDirection.Normalize();
                
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector3.zero;
                    playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                }
                
                // Start cooldown
                canAttack = false;
                attackTimer = 0f;
            }
            else
            {
                Debug.Log("[MeleeGoblin] Collision detected but no damage dealt (out of attack range or on cooldown)");
            }
        }
    }
    
    // Helper method to get bounds of an object
    Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds;
        }
        
        // Fallback to just using position
        return new Bounds(obj.transform.position, Vector3.one);
    }
    
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.Log($"[MeleeGoblin] {gameObject.name} defeated!");
        
        // Stop chase sound
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
        
        // Play death sound if available
        if (deathSFX != null)
        {
            AudioSource.PlayClipAtPoint(deathSFX, transform.position);
        }
        
        // Award score
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(scoreValue);
            Debug.Log($"[MeleeGoblin] Added {scoreValue} points to score");
        }
        
        // Destroy goblin
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
        // Clean up audio
        if (chaseAudioSource != null && chaseAudioSource.isPlaying)
        {
            chaseAudioSource.Stop();
        }
    }
}