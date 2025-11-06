using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Centralized audio management system for all game sound effects
/// Provides singleton access, audio source pooling, and volume control
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private int audioSourcePoolSize = 10;
    [SerializeField] private AudioSource musicSource; // For background music
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip cheeseCollectionSound;
    [SerializeField] private AudioClip mouseDeathSound;
    [SerializeField] private AudioClip gravityShiftSound;
    [SerializeField] private AudioClip catMeowSound;
    [SerializeField] private AudioClip marketOpenSound;
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip buffCollectionSound;
    [SerializeField] private AudioClip mouseHoleSpawnSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip sopaAttackSound;
    [SerializeField] private AudioClip teleportSound;
    
    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.8f;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.6f;
    
    [Header("Audio Source Pool")]
    private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();
    private List<AudioSource> allAudioSources = new List<AudioSource>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Only DontDestroyOnLoad if this is a root object
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the audio manager and creates audio source pool
    /// </summary>
    void InitializeAudioManager()
    {
        // Create audio source pool
        for (int i = 0; i < audioSourcePoolSize; i++)
        {
            GameObject audioSourceObject = new GameObject($"AudioSource_{i}");
            audioSourceObject.transform.SetParent(transform);
            
            AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = sfxVolume * masterVolume;
            
            availableAudioSources.Enqueue(audioSource);
            allAudioSources.Add(audioSource);
        }
        
        // Setup music source if not assigned
        if (musicSource == null)
        {
            GameObject musicObject = new GameObject("MusicSource");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
        }
        
        // Generate fallback clips if none are assigned
        GenerateFallbackClips();
        
        Debug.Log($"AudioManager: Initialized with {audioSourcePoolSize} audio sources");
    }
    
    /// <summary>
    /// Generates simple fallback audio clips when none are assigned
    /// </summary>
    void GenerateFallbackClips()
    {
        if (cheeseCollectionSound == null)
            cheeseCollectionSound = AudioClipGenerator.CreateBeep(800f, 0.1f);
            
        if (mouseDeathSound == null)
            mouseDeathSound = AudioClipGenerator.CreateNoiseBurst(0.3f);
            
        if (gravityShiftSound == null)
            gravityShiftSound = AudioClipGenerator.CreateSweep(300f, 600f, 0.2f);
            
        if (catMeowSound == null)
            catMeowSound = AudioClipGenerator.CreateSweep(400f, 200f, 0.4f);
            
        if (marketOpenSound == null)
            marketOpenSound = AudioClipGenerator.CreateBeep(600f, 0.3f);
            
        if (purchaseSound == null)
            purchaseSound = AudioClipGenerator.CreateBeep(1000f, 0.15f);
            
        if (buffCollectionSound == null)
            buffCollectionSound = AudioClipGenerator.CreateSweep(500f, 1000f, 0.2f);
            
        if (mouseHoleSpawnSound == null)
            mouseHoleSpawnSound = AudioClipGenerator.CreateSweep(200f, 400f, 0.5f);
            
        if (victorySound == null)
            victorySound = AudioClipGenerator.CreateSweep(440f, 880f, 0.8f);
            
        if (sopaAttackSound == null)
            sopaAttackSound = AudioClipGenerator.CreateNoiseBurst(0.15f);
            
        if (teleportSound == null)
            teleportSound = AudioClipGenerator.CreateSweep(800f, 400f, 0.25f);
        
        Debug.Log("AudioManager: Generated fallback audio clips");
    }
    
    /// <summary>
    /// Plays a sound effect using the audio source pool
    /// </summary>
    public void PlaySound(AudioClip clip, float volumeMultiplier = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        
        AudioSource audioSource = GetAvailableAudioSource();
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.volume = sfxVolume * masterVolume * volumeMultiplier;
            audioSource.pitch = pitch;
            audioSource.Play();
            
            // Return to pool after clip finishes
            StartCoroutine(ReturnAudioSourceToPool(audioSource, clip.length / pitch));
        }
        else
        {
            Debug.LogWarning("AudioManager: No available audio sources in pool!");
        }
    }
    
    /// <summary>
    /// Plays cheese collection sound
    /// </summary>
    public void PlayCheeseCollection()
    {
        PlaySound(cheeseCollectionSound, 0.7f, Random.Range(0.9f, 1.1f));
    }
    
    /// <summary>
    /// Plays mouse death sound
    /// </summary>
    public void PlayMouseDeath()
    {
        PlaySound(mouseDeathSound, 1f, 1f);
    }
    
    /// <summary>
    /// Plays gravity shift sound
    /// </summary>
    public void PlayGravityShift()
    {
        PlaySound(gravityShiftSound, 0.6f, Random.Range(0.95f, 1.05f));
    }
    
    /// <summary>
    /// Plays cat meow sound
    /// </summary>
    public void PlayCatMeow()
    {
        PlaySound(catMeowSound, 0.8f, Random.Range(0.8f, 1.2f));
    }
    
    /// <summary>
    /// Plays market open sound
    /// </summary>
    public void PlayMarketOpen()
    {
        PlaySound(marketOpenSound, 0.9f, 1f);
    }
    
    /// <summary>
    /// Plays purchase sound
    /// </summary>
    public void PlayPurchase()
    {
        PlaySound(purchaseSound, 0.8f, 1f);
    }
    
    /// <summary>
    /// Plays buff collection sound
    /// </summary>
    public void PlayBuffCollection()
    {
        PlaySound(buffCollectionSound, 0.7f, Random.Range(1f, 1.3f));
    }
    
    /// <summary>
    /// Plays mouse hole spawn sound
    /// </summary>
    public void PlayMouseHoleSpawn()
    {
        PlaySound(mouseHoleSpawnSound, 1f, 1f);
    }
    
    /// <summary>
    /// Plays victory sound
    /// </summary>
    public void PlayVictory()
    {
        PlaySound(victorySound, 1f, 1f);
    }
    
    /// <summary>
    /// Plays sopa attack sound
    /// </summary>
    public void PlaySopaAttack()
    {
        PlaySound(sopaAttackSound, 0.8f, Random.Range(0.9f, 1.1f));
    }
    
    /// <summary>
    /// Plays teleport sound
    /// </summary>
    public void PlayTeleport()
    {
        PlaySound(teleportSound, 0.7f, Random.Range(0.95f, 1.05f));
    }
    
    /// <summary>
    /// Plays background music
    /// </summary>
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// Stops background music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    /// <summary>
    /// Sets master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    /// <summary>
    /// Sets SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    /// <summary>
    /// Sets music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// Updates all audio source volumes
    /// </summary>
    void UpdateAllVolumes()
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.volume = sfxVolume * masterVolume;
            }
        }
        
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// Gets an available audio source from the pool
    /// </summary>
    AudioSource GetAvailableAudioSource()
    {
        // Try to get from available queue first
        if (availableAudioSources.Count > 0)
        {
            return availableAudioSources.Dequeue();
        }
        
        // If no available sources, find one that's not playing
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }
        
        // If all sources are busy, return the first one (will interrupt)
        return allAudioSources.Count > 0 ? allAudioSources[0] : null;
    }
    
    /// <summary>
    /// Returns an audio source to the pool after it finishes playing
    /// </summary>
    IEnumerator ReturnAudioSourceToPool(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (audioSource != null && !audioSource.isPlaying)
        {
            availableAudioSources.Enqueue(audioSource);
        }
    }
    
    /// <summary>
    /// Stops all currently playing sounds
    /// </summary>
    public void StopAllSounds()
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                availableAudioSources.Enqueue(audioSource);
            }
        }
    }
    
    /// <summary>
    /// Gets current master volume
    /// </summary>
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    /// <summary>
    /// Gets current SFX volume
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    
    /// <summary>
    /// Gets current music volume
    /// </summary>
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    /// <summary>
    /// Gets debug information about audio sources
    /// </summary>
    public string GetDebugInfo()
    {
        int playingSources = 0;
        foreach (AudioSource source in allAudioSources)
        {
            if (source != null && source.isPlaying)
                playingSources++;
        }
        
        return $"Audio Sources: {playingSources}/{allAudioSources.Count} playing, {availableAudioSources.Count} available";
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}