using UnityEngine;

/// <summary>
/// Boss-specific dialogue controller
/// Triggers different dialogues at specific health percentages
/// Perfect for Rantor boss fight with intro, mid-battle, and defeat dialogues
/// </summary>
public class BossDialogueController : MonoBehaviour
{
    [Header("Boss Settings")]
    public string bossName = "Rantor";
    public int maxHealth = 100;
    private int currentHealth;
    
    [Header("Dialogue Triggers")]
    public DialogueData introDialogue;      // Plays at start
    public DialogueData halfHealthDialogue; // Plays at 50% health
    public DialogueData lowHealthDialogue;  // Plays at 25% health
    public DialogueData defeatDialogue;     // Plays at 0% health
    
    [Header("Trigger Settings")]
    public bool autoPlayIntro = true;       // Auto-play intro when player enters
    public float halfHealthThreshold = 0.5f; // 50%
    public float lowHealthThreshold = 0.25f; // 25%
    
    [Header("Advanced Settings")]
    public bool pauseDuringDialogue = true;
    public bool onlyTriggerOnce = true;     // Each dialogue triggers only once
    
    private bool introPlayed = false;
    private bool halfHealthPlayed = false;
    private bool lowHealthPlayed = false;
    private bool defeatPlayed = false;
    private bool bossDefeated = false;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Auto-play intro if enabled
        if (autoPlayIntro && introDialogue != null)
        {
            PlayIntroDialogue();
        }
    }
    
    void Update()
    {
        // Check health-based triggers
        CheckHealthDialogues();
    }
    
    void CheckHealthDialogues()
    {
        if (bossDefeated) return;
        
        float healthPercent = (float)currentHealth / maxHealth;
        
        // Half health dialogue
        if (!halfHealthPlayed && healthPercent <= halfHealthThreshold && halfHealthDialogue != null)
        {
            PlayHalfHealthDialogue();
        }
        
        // Low health dialogue
        if (!lowHealthPlayed && healthPercent <= lowHealthThreshold && lowHealthDialogue != null)
        {
            PlayLowHealthDialogue();
        }
        
        // Defeat dialogue
        if (currentHealth <= 0 && !defeatPlayed)
        {
            PlayDefeatDialogue();
        }
    }
    
    public void PlayIntroDialogue()
    {
        if (introPlayed && onlyTriggerOnce) return;
        
        if (introDialogue != null && DialogueManager.instance != null)
        {
            Debug.Log(bossName + " Who goes there??? You dare tresspass into my throne room!?!? You'll pay with your life!");
            DialogueManager.instance.StartDialogue(introDialogue);
            introPlayed = true;
        }
    }
    
    public void PlayHalfHealthDialogue()
    {
        if (halfHealthPlayed && onlyTriggerOnce) return;
        
        if (halfHealthDialogue != null && DialogueManager.instance != null)
        {
            Debug.Log(bossName + " How are you still breathing!");
            
            if (pauseDuringDialogue)
            {
                Time.timeScale = 0f;
            }
            
            DialogueManager.instance.StartDialogue(halfHealthDialogue);
            halfHealthPlayed = true;
        }
    }
    
    public void PlayLowHealthDialogue()
    {
        if (lowHealthPlayed && onlyTriggerOnce) return;
        
        if (lowHealthDialogue != null && DialogueManager.instance != null)
        {
            Debug.Log(bossName + " You haven't bested me yet.");
            
            if (pauseDuringDialogue)
            {
                Time.timeScale = 0f;
            }
            
            DialogueManager.instance.StartDialogue(lowHealthDialogue);
            lowHealthPlayed = true;
        }
    }
    
    public void PlayDefeatDialogue()
    {
        if (defeatPlayed && onlyTriggerOnce) return;
        
        if (defeatDialogue != null && DialogueManager.instance != null)
        {
            Debug.Log(bossName + " no way...");
            bossDefeated = true;
            
            // Always pause for defeat dialogue
            Time.timeScale = 0f;
            
            DialogueManager.instance.StartDialogue(defeatDialogue);
            defeatPlayed = true;
        }
    }
    
    // Public method to take damage - call this from your boss fight script
    public void TakeDamage(int damage)
    {
        if (bossDefeated) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Don't go below 0
        
        Debug.Log(bossName + " health: " + currentHealth + "/" + maxHealth);
        
        // Check if boss defeated
        if (currentHealth <= 0)
        {
            OnBossDefeated();
        }
    }
    
    void OnBossDefeated()
    {
        bossDefeated = true;
        Debug.Log(bossName + " has been defeated!");
        
        // Play defeat dialogue
        PlayDefeatDialogue();
        
        // Add any other defeat logic here
        // For example: award score, unlock next level, etc.
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(100); // Big score for defeating boss
        }
    }
    
    // Public getters
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    
    public bool IsBossDefeated()
    {
        return bossDefeated;
    }
    
    // Reset boss (useful for testing or game restart)
    public void ResetBoss()
    {
        currentHealth = maxHealth;
        bossDefeated = false;
        introPlayed = false;
        halfHealthPlayed = false;
        lowHealthPlayed = false;
        defeatPlayed = false;
        Time.timeScale = 1f;
    }
    
    // Manual trigger methods (can be called from other scripts)
    public void TriggerIntro()
    {
        introPlayed = false;
        PlayIntroDialogue();
    }
    
    // Example collision detection for intro trigger
    void OnTriggerEnter(Collider other)
    {
        // Check if player entered boss arena
        if (other.GetComponent<PlayerHealth>() != null || 
            other.GetComponent<PlayerPhysics>() != null)
        {
            if (autoPlayIntro && !introPlayed)
            {
                PlayIntroDialogue();
            }
        }
    }
}