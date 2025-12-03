using UnityEngine;

/// <summary>
/// Optional helper script - Add to player to automatically disable movement during dialogue
/// This prevents the player from walking around while talking to NPCs
/// </summary>
public class DialoguePlayerController : MonoBehaviour
{
    [Header("Components to Disable During Dialogue")]
    public PlayerPhysics playerPhysics;
    public PlayerBehaviour playerBehaviour;
    
    [Header("Settings")]
    public bool disableMovementDuringDialogue = true;
    public bool disableJumpingDuringDialogue = true;
    
    private bool wasPhysicsEnabled = true;
    private bool wasBehaviourEnabled = true;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (playerPhysics == null)
        {
            playerPhysics = GetComponent<PlayerPhysics>();
        }
        
        if (playerBehaviour == null)
        {
            playerBehaviour = GetComponent<PlayerBehaviour>();
        }
    }
    
    void Update()
    {
        // Check if dialogue is active
        if (DialogueManager.instance != null)
        {
            bool dialogueActive = DialogueManager.instance.IsDialogueActive();
            
            if (dialogueActive && disableMovementDuringDialogue)
            {
                // Dialogue started - disable player controls
                DisablePlayerControls();
            }
            else if (!dialogueActive)
            {
                // Dialogue ended - restore player controls
                EnablePlayerControls();
            }
        }
    }
    
    void DisablePlayerControls()
    {
        if (playerPhysics != null && playerPhysics.enabled)
        {
            wasPhysicsEnabled = true;
            playerPhysics.enabled = false;
        }
        
        if (playerBehaviour != null && playerBehaviour.enabled)
        {
            wasBehaviourEnabled = true;
            playerBehaviour.enabled = false;
        }
    }
    
    void EnablePlayerControls()
    {
        if (playerPhysics != null && wasPhysicsEnabled)
        {
            playerPhysics.enabled = true;
            wasPhysicsEnabled = false;
        }
        
        if (playerBehaviour != null && wasBehaviourEnabled)
        {
            playerBehaviour.enabled = true;
            wasBehaviourEnabled = false;
        }
    }
    
    void OnDisable()
    {
        // Make sure controls are re-enabled if this script is disabled
        EnablePlayerControls();
    }
}