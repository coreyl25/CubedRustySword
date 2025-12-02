using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;
    public float jumpForce = 12f; // Increased for higher jumps
    public float doubleJumpForce = 10f; // Force for second jump (usually slightly less)
    public float airControlMultiplier = 0.75f; // Controls how much movement is allowed in air
    public float gravityMultiplier = 2f; // Adjust gravity strength (lower = floatier)
    
    private bool isGrounded = true;
    private bool hasDoubleJump = true; // Tracks if double jump is available
    private int jumpCount = 0; // Track number of jumps performed
    private Vector3 originalPosition;
    private Rigidbody rb;
    
    // NEW: Add jump buffer to prevent immediate re-grounding
    private float coyoteTime = 0.1f; // Time after leaving ground where you can still jump
    private float coyoteTimeCounter = 0f;
    private float jumpBufferTime = 0.2f; // Time window to buffer jump input
    private float jumpBufferCounter = 0f;
    private float lastJumpTime = 0f; // Track when we last jumped
    private float groundingDelay = 0.15f; // Delay before we can be grounded again after jumping
    
    void Start()
    {
        originalPosition = transform.position;
        
        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerPhysics requires a Rigidbody component!");
        }
        else
        {
            // Configure Rigidbody for better physics
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
        }
    }
    
    void Update()
    {
        // Don't allow movement when paused
        if (Time.timeScale == 0f) return;
        
        HandleMovement();
        HandleRotation();
        HandleJump();
        
        // Update coyote time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        // Update jump buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    
    void FixedUpdate()
    {
        // Apply additional gravity for more responsive jumping
        if (rb != null && !isGrounded)
        {
            rb.AddForce(Vector3.down * gravityMultiplier * Physics.gravity.magnitude, ForceMode.Acceleration);
        }
    }
    
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(h, 0, v).normalized;
        
        // Apply air control - reduce movement speed when not grounded
        float currentSpeed = isGrounded ? speed : speed * airControlMultiplier;
        
        // Use Rigidbody for movement to maintain momentum
        if (rb != null && direction != Vector3.zero)
        {
            Vector3 moveVelocity = direction * currentSpeed;
            // Only modify X and Z velocity, keep Y velocity (for jumping)
            rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        }
        
        // Rotation to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }
    }
    
    void HandleRotation()
    {
        // Manual rotation inputs (if you want additional rotation control)
        float rotation = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            rotation = -rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotation = rotationSpeed * Time.deltaTime;
        }
        
        // Apply Y-axis rotation (left/right)
        if (rotation != 0f)
        {
            transform.Rotate(Vector3.up, rotation);
        }
    }
    
    void HandleJump()
    {
        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && rb != null)
        {
            Debug.Log($"Space pressed - isGrounded: {isGrounded}, jumpCount: {jumpCount}, hasDoubleJump: {hasDoubleJump}");
            
            // First jump - when grounded OR within coyote time
            if ((isGrounded || coyoteTimeCounter > 0f) && jumpCount == 0)
            {
                PerformJump(jumpForce);
                jumpCount = 1;
                isGrounded = false; // Force isGrounded to false after jumping
                coyoteTimeCounter = 0f; // Reset coyote time
                lastJumpTime = Time.time; // Record jump time
                Debug.Log("First jump!");
            }
            // Double jump - when in air and haven't used double jump yet
            else if (!isGrounded && jumpCount == 1 && hasDoubleJump)
            {
                PerformJump(doubleJumpForce);
                jumpCount = 2;
                hasDoubleJump = false;
                lastJumpTime = Time.time; // Record jump time
                Debug.Log("Double jump!");
            }
            else
            {
                Debug.Log($"Jump blocked - Reason: isGrounded={isGrounded}, jumpCount={jumpCount}, hasDoubleJump={hasDoubleJump}");
            }
        }
    }
    
    void PerformJump(float force)
    {
        // Reset Y velocity before applying jump force for consistent jump height
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;
        rb.linearVelocity = velocity;
        
        // Apply upward force
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        isGrounded = false;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Only process grounding if enough time has passed since last jump
        if (Time.time - lastJumpTime < groundingDelay)
        {
            Debug.Log("Too soon after jump to be grounded");
            return;
        }
        
        // Check if we're landing on something below us
        foreach (ContactPoint contact in collision.contacts)
        {
            // If the contact normal is pointing mostly upward, we're grounded
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                jumpCount = 0; // Reset jump count
                hasDoubleJump = true; // Restore double jump
                Debug.Log("Player landed - jumps reset");
                break;
            }
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Only process grounding if enough time has passed since last jump
        if (Time.time - lastJumpTime < groundingDelay)
        {
            return;
        }
        
        // Continuously check if grounded while in contact
        bool wasGrounded = isGrounded;
        
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                if (!wasGrounded)
                {
                    isGrounded = true;
                    jumpCount = 0;
                    hasDoubleJump = true;
                    Debug.Log("Player grounded during collision stay");
                }
                else
                {
                    isGrounded = true;
                }
                return;
            }
        }
    }
    
    void OnCollisionExit(Collision collision)
    {
        // When we leave a collision, might not be grounded anymore
        // Check if we're still touching ground with another collider
        CheckGroundedStatus();
    }
    
    void CheckGroundedStatus()
    {
        // Only check if enough time has passed since last jump
        if (Time.time - lastJumpTime < groundingDelay)
        {
            isGrounded = false;
            return;
        }
        
        // Cast a small ray downward to check if still grounded
        RaycastHit hit;
        float rayDistance = 0.1f;
        
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}