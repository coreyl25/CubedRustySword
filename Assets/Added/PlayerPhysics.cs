using UnityEngine;
public class PlayerPhysics : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 100f;
    public float jumpHeight = 5f;
    public float airControlMultiplier = 0.75f; // Controls how much movement is allowed in air
    private bool isGrounded = true;
    private Vector3 originalPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.position;
    }
    
    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
    }
    
    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(h, 0, v).normalized;
        
        // Apply air control - reduce movement speed when not grounded
        float currentSpeed = isGrounded ? speed : speed * airControlMultiplier;
        
        Vector3 move = direction * currentSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);
        
        // Rotation to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }
    }
    
    void HandleRotation()
    {
        // Note: This method now handles manual rotation inputs
        // The automatic rotation to face movement direction is handled in HandleMovement()
        float rotation = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            rotation = -rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rotation = rotationSpeed * Time.deltaTime;
        }
        
        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
        }
        
        // Apply Y-axis rotation (left/right)
        if (rotation != 0f)
        {
            transform.Rotate(Vector3.up, rotation);
        }
    }
    
    void HandleJump()
    {
        // Simple jump mechanic using Translate
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            StartCoroutine(JumpCoroutine());
        }
    }
    
    System.Collections.IEnumerator JumpCoroutine()
    {
        isGrounded = false;
        float jumpTime = 0f;
        float jumpDuration = 0.5f; // Total time for jump
        Vector3 startPos = transform.position;
        
        // Jump up and down using a simple arc
        while (jumpTime < jumpDuration)
        {
            jumpTime += Time.deltaTime;
            float progress = jumpTime / jumpDuration;
            
            // Create parabolic motion for jump
            float yOffset = jumpHeight * (4f * progress * (1f - progress));
            Vector3 newPos = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
            transform.position = newPos;
            yield return null;
        }
        
        // Ensure we land exactly at the starting height
        transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);
        isGrounded = true;
    }
}