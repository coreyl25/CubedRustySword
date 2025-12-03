using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC Info")]
    public string npcName = "NPC";
    public NPCType npcType = NPCType.Friendly;
    
    [Header("Visual Settings")]
    public Color npcColor = Color.blue;
    
    [Header("References")]
    private DialogueTrigger dialogueTrigger;
    private Renderer rend;
    
    public enum NPCType
    {
        Friendly,   // Sigbert
        Boss,       // Rantor
        Enemy,
        Neutral
    }
    
    void Start()
    {
        // Get components
        rend = GetComponent<Renderer>();
        dialogueTrigger = GetComponent<DialogueTrigger>();
        
        // Set color
        if (rend != null)
        {
            rend.material.color = npcColor;
        }
        
        // Log NPC creation
        Debug.Log($"{npcName} ({npcType}) created");
    }
    
    void Update()
    {
        // You can add NPC-specific behaviors here
        // For example, Sigbert could rotate slowly, Rantor could have idle animations, etc.
    }
    
    // Method to start conversation (can be called by other scripts)
    public void StartConversation()
    {
        if (dialogueTrigger != null)
        {
            dialogueTrigger.TriggerDialogue();
        }
        else
        {
            Debug.LogWarning($"{npcName} has no DialogueTrigger component!");
        }
    }
}