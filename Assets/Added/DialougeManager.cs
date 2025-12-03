using UnityEngine;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public GameObject continueIndicator;
    
    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;
    public bool useTypingEffect = true;
    
    [Header("Audio Settings")]
    public AudioClip sigbertVoiceSFX; // For Sigbert and "???"
    public AudioClip russellVoiceSFX; // For Russell
    public float voiceVolume = 0.5f;
    
    private AudioSource voiceAudioSource;
    private DialogueData currentDialogue;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;
    private bool audioInitialized = false;
    
    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Initialize AudioSource in Start() instead of Awake() to avoid FMOD initialization conflicts
        InitializeAudioSource();
        
        // Hide dialogue panel at start
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }
    
    void InitializeAudioSource()
    {
        // Check if AudioSource already exists
        voiceAudioSource = GetComponent<AudioSource>();
        
        if (voiceAudioSource == null)
        {
            // Create AudioSource for voice effects
            voiceAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure AudioSource
        voiceAudioSource.loop = true; // Voice plays continuously during dialogue
        voiceAudioSource.volume = voiceVolume;
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.priority = 128; // Lower priority to avoid conflicts
        
        audioInitialized = true;
        Debug.Log("DialogueManager audio initialized successfully");
    }
    
    void Update()
    {
        if (!dialogueActive) return;
        
        // Check for input to advance dialogue
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // Skip typing animation
                CompleteTyping();
            }
            else
            {
                // Move to next line
                AdvanceDialogue();
            }
        }
    }
    
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Length == 0)
        {
            Debug.LogWarning("Dialogue is empty or null!");
            return;
        }
        
        // Ensure audio is initialized
        if (!audioInitialized)
        {
            InitializeAudioSource();
        }
        
        currentDialogue = dialogue;
        currentLineIndex = 0;
        dialogueActive = true;
        
        // Pause game if specified
        if (currentDialogue.pauseGame)
        {
            Time.timeScale = 0f;
        }
        
        // Show dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
        
        // Display first line
        DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
    }
    
    void DisplayLine(DialogueLine line)
    {
        // Stop any currently playing voice
        StopVoiceAudio();
        
        // Set speaker name
        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
        }
        
        // Select and play appropriate voice SFX based on speaker
        PlayVoiceForSpeaker(line.speakerName);
        
        // Display dialogue with or without typing effect
        if (useTypingEffect)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeDialogue(line.dialogueText));
        }
        else
        {
            dialogueText.text = line.dialogueText;
            isTyping = false;
            ShowContinueIndicator();
            // Stop voice immediately if no typing effect
            StopVoiceAudio();
        }
    }
    
    void PlayVoiceForSpeaker(string speakerName)
    {
        // Don't play audio if not initialized
        if (!audioInitialized || voiceAudioSource == null)
        {
            return;
        }
        
        AudioClip voiceClip = null;
        
        // Determine which voice to use based on speaker name
        // Case-insensitive comparison
        string speaker = speakerName.ToLower().Trim();
        
        if (speaker == "sigbert" || speaker == "???" || speaker.Contains("sigbert"))
        {
            voiceClip = sigbertVoiceSFX;
        }
        else if (speaker == "russell" || speaker == "rusty" || speaker.Contains("russell") || speaker.Contains("rusty"))
        {
            voiceClip = russellVoiceSFX;
        }
        
        // Play the voice clip with additional safety checks
        if (voiceClip != null && voiceAudioSource != null)
        {
            try
            {
                voiceAudioSource.clip = voiceClip;
                voiceAudioSource.Play();
                Debug.Log("Playing voice for: " + speakerName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to play voice audio: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("No voice clip assigned for speaker: " + speakerName);
        }
    }
    
    void StopVoiceAudio()
    {
        if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            try
            {
                voiceAudioSource.Stop();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Failed to stop voice audio: " + e.Message);
            }
        }
    }
    
    IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            
            // Use unscaled time if game is paused
            if (currentDialogue.pauseGame)
            {
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }
        
        // Stop voice when typing is complete
        StopVoiceAudio();
        
        isTyping = false;
        ShowContinueIndicator();
    }
    
    void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        
        // Stop voice when skipping
        StopVoiceAudio();
        
        dialogueText.text = currentDialogue.dialogueLines[currentLineIndex].dialogueText;
        isTyping = false;
        ShowContinueIndicator();
    }
    
    void ShowContinueIndicator()
    {
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }
    
    void AdvanceDialogue()
    {
        // Stop voice when advancing
        StopVoiceAudio();
        
        currentLineIndex++;
        
        // Check if there are more lines
        if (currentLineIndex < currentDialogue.dialogueLines.Length)
        {
            DisplayLine(currentDialogue.dialogueLines[currentLineIndex]);
        }
        else
        {
            EndDialogue();
        }
    }
    
    void EndDialogue()
    {
        // Stop voice when ending dialogue
        StopVoiceAudio();
        
        dialogueActive = false;
        currentDialogue = null;
        currentLineIndex = 0;
        
        // Hide dialogue panel
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
        
        // Unpause game if it was paused
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        
        Debug.Log("Dialogue ended");
    }
    
    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
    
    // Force end dialogue (useful for interruptions)
    public void ForceEndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        StopVoiceAudio();
        EndDialogue();
    }
    
    // Public method to change voice volume
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        if (voiceAudioSource != null)
        {
            voiceAudioSource.volume = voiceVolume;
        }
    }
    
    void OnDestroy()
    {
        // Clean up audio when destroyed
        StopVoiceAudio();
    }
}