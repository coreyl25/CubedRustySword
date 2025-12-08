using UnityEngine;

/// <summary>
/// AudioManager - Handles all game audio including background music
/// Add this to a GameObject in your scene (e.g., "AudioManager")
/// Singleton pattern ensures only one instance exists
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("Background Music")]
    public AudioClip levelMusic; // Assign your background music here
    public float musicVolume = 0.5f;
    public bool loopMusic = true;
    public bool playMusicOnStart = true;
    
    [Header("Player Sound Effects")]
    public AudioClip playerHurtSFX; // NEW: Player takes damage sound
    public AudioClip playerDeathSFX; // NEW: Player dies sound
    public float playerSFXVolume = 0.8f;
    
    [Header("Game Sound Effects")]
    public AudioClip buttonClickSFX;
    public AudioClip gameOverSFX;
    public AudioClip victoryFanfare;
    public float sfxVolume = 0.7f;
    
    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource playerSFXSource; // NEW: Dedicated source for player sounds
    
    void Awake()
    {
        // Singleton pattern - ensures only one AudioManager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persists between scenes (optional)
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Create audio sources
        InitializeAudioSources();
    }
    
    void Start()
    {
        // Play background music on start if enabled
        if (playMusicOnStart && levelMusic != null)
        {
            PlayMusic();
        }
    }
    
    void InitializeAudioSources()
    {
        // Create AudioSource for background music
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = loopMusic;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
        musicSource.priority = 0; // Highest priority
        
        // Create AudioSource for sound effects
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
        sfxSource.playOnAwake = false;
        sfxSource.priority = 128; // Normal priority
        
        // NEW: Create AudioSource for player sounds
        playerSFXSource = gameObject.AddComponent<AudioSource>();
        playerSFXSource.loop = false;
        playerSFXSource.volume = playerSFXVolume;
        playerSFXSource.playOnAwake = false;
        playerSFXSource.priority = 64; // High priority (player sounds are important!)
        
        Debug.Log("[AudioManager] Audio sources initialized");
    }
    
    // ===== MUSIC CONTROLS =====
    
    public void PlayMusic()
    {
        if (levelMusic == null)
        {
            Debug.LogWarning("[AudioManager] No music assigned to Level Music!");
            return;
        }
        
        if (musicSource.isPlaying)
        {
            Debug.Log("[AudioManager] Music already playing");
            return;
        }
        
        musicSource.clip = levelMusic;
        musicSource.Play();
        Debug.Log("[AudioManager] Started playing: " + levelMusic.name);
    }
    
    public void StopMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[AudioManager] Music stopped");
        }
    }
    
    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            Debug.Log("[AudioManager] Music paused");
        }
    }
    
    public void ResumeMusic()
    {
        if (!musicSource.isPlaying && musicSource.clip != null)
        {
            musicSource.UnPause();
            Debug.Log("[AudioManager] Music resumed");
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        Debug.Log("[AudioManager] Music volume set to: " + musicVolume);
    }
    
    public void FadeOutMusic(float duration = 2f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }
    
    System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time for paused game
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        musicSource.Stop();
        musicSource.volume = startVolume; // Reset volume for next play
        Debug.Log("[AudioManager] Music faded out");
    }
    
    // ===== PLAYER SOUND EFFECTS (NEW) =====
    
    public void PlayPlayerHurt()
    {
        if (playerHurtSFX != null)
        {
            playerSFXSource.PlayOneShot(playerHurtSFX, playerSFXVolume);
            Debug.Log("[AudioManager] Player hurt sound played");
        }
        else
        {
            Debug.LogWarning("[AudioManager] Player Hurt SFX not assigned!");
        }
    }
    
    public void PlayPlayerDeath()
    {
        if (playerDeathSFX != null)
        {
            playerSFXSource.PlayOneShot(playerDeathSFX, playerSFXVolume);
            Debug.Log("[AudioManager] Player death sound played");
        }
        else
        {
            Debug.LogWarning("[AudioManager] Player Death SFX not assigned!");
        }
    }
    
    public void SetPlayerSFXVolume(float volume)
    {
        playerSFXVolume = Mathf.Clamp01(volume);
        playerSFXSource.volume = playerSFXVolume;
        Debug.Log("[AudioManager] Player SFX volume set to: " + playerSFXVolume);
    }
    
    // ===== GAME SOUND EFFECTS =====
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null sound effect");
            return;
        }
        
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    
    public void PlaySFX(AudioClip clip, float volumeMultiplier)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to play null sound effect");
            return;
        }
        
        sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }
    
    public void PlayButtonClick()
    {
        if (buttonClickSFX != null)
        {
            PlaySFX(buttonClickSFX);
        }
    }
    
    public void PlayGameOver()
    {
        if (gameOverSFX != null)
        {
            PlaySFX(gameOverSFX);
        }
    }
    
    public void PlayVictory()
    {
        if (victoryFanfare != null)
        {
            PlaySFX(victoryFanfare);
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
        Debug.Log("[AudioManager] SFX volume set to: " + sfxVolume);
    }
    
    // ===== UTILITY METHODS =====
    
    public bool IsMusicPlaying()
    {
        return musicSource.isPlaying;
    }
    
    public void MuteAll()
    {
        musicSource.mute = true;
        sfxSource.mute = true;
        playerSFXSource.mute = true;
        Debug.Log("[AudioManager] All audio muted");
    }
    
    public void UnmuteAll()
    {
        musicSource.mute = false;
        sfxSource.mute = false;
        playerSFXSource.mute = false;
        Debug.Log("[AudioManager] All audio unmuted");
    }
    
    // Change music (useful for different levels or situations)
    public void ChangeMusic(AudioClip newMusic)
    {
        if (newMusic == null)
        {
            Debug.LogWarning("[AudioManager] Attempted to change to null music");
            return;
        }
        
        musicSource.Stop();
        levelMusic = newMusic;
        musicSource.clip = newMusic;
        musicSource.Play();
        Debug.Log("[AudioManager] Changed music to: " + newMusic.name);
    }
    
    void OnDestroy()
    {
        // Clean up when destroyed
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}