using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    
    // Camera view offsets
    public Vector3 topDownOffset = new Vector3(0, 10, 0);
    public Vector3 thirdPersonOffset = new Vector3(0, 5, -7);
    
    public float smoothSpeed = 5f;
    
    private bool isTopDown = false;
    
    void Start()
    {
        // Start in third-person view by default
        isTopDown = false;
    }
    
    void Update()
    {
        // Toggle between views when V key is pressed
        if (Input.GetKeyDown(KeyCode.V))
        {
            isTopDown = !isTopDown;
            Debug.Log("Camera View: " + (isTopDown ? "Top-Down" : "Third-Person"));
        }
    }
    
    void LateUpdate()
    {
        // Use the active offset based on current view mode
        Vector3 activeOffset = isTopDown ? topDownOffset : thirdPersonOffset;
        
        // Calculate desired camera position
        Vector3 desiredPosition = player.position + activeOffset;
        
        // Smoothly move camera to desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Always look at the player
        transform.LookAt(player);
    }
}