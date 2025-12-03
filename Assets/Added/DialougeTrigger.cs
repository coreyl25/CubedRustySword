using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueData dialogue;
    public bool triggerOnCollision = false; // Auto-trigger when player touches
    public bool triggerOnInteraction = true; // Trigger when player presses E nearby
    public float interactionRange = 3f;
    
    [Header("Visual Feedback")]
    public GameObject interactionPrompt; // UI element like "Press E to talk"
    
    [Header("One-Time Dialogue")]
    public bool oneTimeOnly = false;
    private bool hasTriggered = false;
    
    private Transform player;
    private bool playerInRange = false;
    
    void Start()
    {
        // Find player
        GameObject playerObj = FindPlayerObject();
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Hide interaction prompt at start
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    GameObject FindPlayerObject()
    {
        // Try to find player by various methods
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            return playerHealth.gameObject;
        }
        
        PlayerPhysics playerPhysics = FindObjectOfType<PlayerPhysics>();
        if (playerPhysics != null)
        {
            return playerPhysics.gameObject;
        }
        
        GameObject player = GameObject.Find("Player");
        if (player != null) return player;
        
        player = GameObject.Find("Capsule");
        if (player != null) return player;
        
        return null;
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;
        
        // Show/hide interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(playerInRange && !DialogueManager.instance.IsDialogueActive());
        }
        
        // Check for interaction input
        if (triggerOnInteraction && playerInRange && !hasTriggered)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TriggerDialogue();
            }
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!triggerOnCollision || hasTriggered) return;
        
        // Check if player collided
        if (collision.gameObject.GetComponent<PlayerHealth>() != null || 
            collision.gameObject.GetComponent<PlayerPhysics>() != null)
        {
            TriggerDialogue();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnCollision || hasTriggered) return;
        
        // Check if player entered trigger
        if (other.GetComponent<PlayerHealth>() != null || 
            other.GetComponent<PlayerPhysics>() != null)
        {
            TriggerDialogue();
        }
    }
    
    public void TriggerDialogue()
    {
        if (dialogue == null)
        {
            Debug.LogWarning("No dialogue assigned to " + gameObject.name);
            return;
        }
        
        if (oneTimeOnly && hasTriggered)
        {
            Debug.Log("Dialogue already triggered (one-time only)");
            return;
        }
        
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.StartDialogue(dialogue);
            
            if (oneTimeOnly)
            {
                hasTriggered = true;
            }
            
            // Hide interaction prompt during dialogue
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("DialogueManager not found in scene!");
        }
    }
    
    // Visualize interaction range in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
    
    // Public method to reset dialogue (useful for testing or repeatable dialogues)
    public void ResetDialogue()
    {
        hasTriggered = false;
    }
}