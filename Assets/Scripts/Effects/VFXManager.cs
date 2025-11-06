using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Centralized visual effects management system for all game particle effects
/// Provides singleton access, particle effect pooling, and performance optimization
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Effect Prefabs")]
    [SerializeField] private GameObject cheeseScoreEffectPrefab;
    [SerializeField] private GameObject mouseDeathEffectPrefab;
    [SerializeField] private GameObject buffActivationEffectPrefab;
    [SerializeField] private GameObject teleportEffectPrefab;
    [SerializeField] private GameObject sopaAttackEffectPrefab;
    [SerializeField] private GameObject mouseHoleSpawnEffectPrefab;
    
    [Header("Auto-Create Missing Prefabs")]
    [SerializeField] private bool autoCreateMissingPrefabs = true;
    
    [Header("Effect Pool Settings")]
    [SerializeField] private int effectPoolSize = 15;
    [SerializeField] private float defaultEffectDuration = 2f;
    
    [Header("Performance Settings")]
    [SerializeField] private int maxActiveEffects = 20;
    [SerializeField] private bool enableEffectPooling = true;
    
    // Object pools for different effect types
    private Queue<GameObject> cheeseScoreEffectPool = new Queue<GameObject>();
    private Queue<GameObject> deathEffectPool = new Queue<GameObject>();
    private Queue<GameObject> buffEffectPool = new Queue<GameObject>();
    private Queue<GameObject> teleportEffectPool = new Queue<GameObject>();
    private Queue<GameObject> sopaEffectPool = new Queue<GameObject>();
    private Queue<GameObject> mouseHoleEffectPool = new Queue<GameObject>();
    
    // Active effects tracking
    private List<GameObject> activeEffects = new List<GameObject>();
    
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
            InitializeVFXManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the VFX manager and creates effect pools
    /// </summary>
    void InitializeVFXManager()
    {
        if (enableEffectPooling)
        {
            CreateEffectPools();
        }
        
        Debug.Log($"VFXManager: Initialized with pooling {(enableEffectPooling ? "enabled" : "disabled")}");
    }
    
    /// <summary>
    /// Creates object pools for all effect types
    /// </summary>
    void CreateEffectPools()
    {
        int poolSizePerType = effectPoolSize / 6; // Divide among 6 effect types
        
        // Create pools for each effect type
        CreateEffectPool(cheeseScoreEffectPrefab, cheeseScoreEffectPool, poolSizePerType, "CheeseScoreEffect");
        CreateEffectPool(mouseDeathEffectPrefab, deathEffectPool, poolSizePerType, "DeathEffect");
        CreateEffectPool(buffActivationEffectPrefab, buffEffectPool, poolSizePerType, "BuffEffect");
        CreateEffectPool(teleportEffectPrefab, teleportEffectPool, poolSizePerType, "TeleportEffect");
        CreateEffectPool(sopaAttackEffectPrefab, sopaEffectPool, poolSizePerType, "SopaEffect");
        CreateEffectPool(mouseHoleSpawnEffectPrefab, mouseHoleEffectPool, poolSizePerType, "MouseHoleEffect");
    }
    
    /// <summary>
    /// Creates a pool for a specific effect type
    /// </summary>
    void CreateEffectPool(GameObject prefab, Queue<GameObject> pool, int poolSize, string effectName)
    {
        if (prefab == null) return;
        
        GameObject poolParent = new GameObject($"{effectName}_Pool");
        poolParent.transform.SetParent(transform);
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(prefab, poolParent.transform);
            effect.SetActive(false);
            pool.Enqueue(effect);
        }
    }
    /// <summary>
    /// Plays cheese score collection effect at specified position
    /// </summary>
    public void PlayCheeseScoreEffect(Vector3 position)
    {
        PlayEffect(cheeseScoreEffectPrefab, cheeseScoreEffectPool, position, "CheeseScore");
    }
    
    /// <summary>
    /// Plays mouse death explosion/scatter particle effect at specified position
    /// </summary>
    public void PlayDeathEffect(Vector3 position)
    {
        PlayEffect(mouseDeathEffectPrefab, deathEffectPool, position, "MouseDeath");
    }
    
    /// <summary>
    /// Plays buff activation visual feedback effect at specified position
    /// </summary>
    public void PlayBuffActivationEffect(Vector3 position)
    {
        PlayEffect(buffActivationEffectPrefab, buffEffectPool, position, "BuffActivation");
    }
    
    /// <summary>
    /// Plays teleport effect at specified position
    /// </summary>
    public void PlayTeleportEffect(Vector3 position)
    {
        PlayEffect(teleportEffectPrefab, teleportEffectPool, position, "Teleport");
    }
    
    /// <summary>
    /// Plays sopa attack effect at specified position
    /// </summary>
    public void PlaySopaAttackEffect(Vector3 position)
    {
        PlayEffect(sopaAttackEffectPrefab, sopaEffectPool, position, "SopaAttack");
    }
    
    /// <summary>
    /// Plays mouse hole spawn effect at specified position
    /// </summary>
    public void PlayMouseHoleSpawnEffect(Vector3 position)
    {
        PlayEffect(mouseHoleSpawnEffectPrefab, mouseHoleEffectPool, position, "MouseHoleSpawn");
    }
    
    /// <summary>
    /// Generic method to play any effect with pooling support
    /// </summary>
    void PlayEffect(GameObject effectPrefab, Queue<GameObject> effectPool, Vector3 position, string effectName)
    {
        if (effectPrefab == null)
        {
            if (autoCreateMissingPrefabs)
            {
                effectPrefab = CreateMissingEffectPrefab(effectName);
                AssignCreatedPrefab(effectName, effectPrefab);
            }
            
            if (effectPrefab == null)
            {
                Debug.LogWarning($"VFXManager: {effectName} effect prefab is not assigned and auto-creation failed. Effect will not play.");
                return;
            }
        }
        
        Debug.Log($"VFXManager: PlayEffect called for {effectName} at {position}");
        
        // Check if we've reached the maximum active effects limit
        if (activeEffects.Count >= maxActiveEffects)
        {
            CleanupOldestEffect();
        }
        
        GameObject effect = null;
        
        if (enableEffectPooling && effectPool.Count > 0)
        {
            // Get from pool
            effect = effectPool.Dequeue();
            effect.transform.position = position;
            effect.SetActive(true);
            Debug.Log($"VFXManager: Using pooled effect for {effectName} at {position}");
        }
        else
        {
            // Create new instance
            effect = Instantiate(effectPrefab, position, Quaternion.identity);
            effect.SetActive(true);
            Debug.Log($"VFXManager: Created new effect instance for {effectName} at {position}");
        }
        
        if (effect != null)
        {
            activeEffects.Add(effect);
            Debug.Log($"VFXManager: Effect {effectName} activated at {position}");
            
            // Start coroutine to handle effect cleanup
            StartCoroutine(HandleEffectLifetime(effect, effectPool, defaultEffectDuration));
        }
        else
        {
            Debug.LogError($"VFXManager: Failed to create effect {effectName}");
        }
    }
    

    
    /// <summary>
    /// Handles the lifetime of an effect and returns it to pool when done
    /// </summary>
    IEnumerator HandleEffectLifetime(GameObject effect, Queue<GameObject> pool, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (effect != null)
        {
            ReturnEffectToPool(effect, pool);
        }
    }
    
    /// <summary>
    /// Returns an effect to its appropriate pool
    /// </summary>
    void ReturnEffectToPool(GameObject effect, Queue<GameObject> pool)
    {
        if (effect == null) return;
        
        activeEffects.Remove(effect);
        
        if (enableEffectPooling)
        {
            effect.SetActive(false);
            pool.Enqueue(effect);
        }
        else
        {
            Destroy(effect);
        }
    }
    
    /// <summary>
    /// Cleans up the oldest active effect to make room for new ones
    /// </summary>
    void CleanupOldestEffect()
    {
        if (activeEffects.Count > 0)
        {
            GameObject oldestEffect = activeEffects[0];
            if (oldestEffect != null)
            {
                activeEffects.RemoveAt(0);
                
                if (enableEffectPooling)
                {
                    oldestEffect.SetActive(false);
                    // Note: We don't know which pool this belongs to, so we'll just destroy it
                    Destroy(oldestEffect);
                }
                else
                {
                    Destroy(oldestEffect);
                }
            }
        }
    }
    
    /// <summary>
    /// Stops all currently active effects
    /// </summary>
    public void StopAllEffects()
    {
        foreach (GameObject effect in activeEffects)
        {
            if (effect != null)
            {
                if (enableEffectPooling)
                {
                    effect.SetActive(false);
                }
                else
                {
                    Destroy(effect);
                }
            }
        }
        
        activeEffects.Clear();
        
        // Return all effects to their pools
        if (enableEffectPooling)
        {
            RefillAllPools();
        }
    }
    
    /// <summary>
    /// Refills all effect pools (used when stopping all effects)
    /// </summary>
    void RefillAllPools()
    {
        // This is a simplified approach - in a more complex system,
        // we'd track which pool each effect belongs to
        CreateEffectPools();
    }
    
    /// <summary>
    /// Sets whether effect pooling is enabled
    /// </summary>
    public void SetEffectPooling(bool enabled)
    {
        enableEffectPooling = enabled;
        
        if (enabled && cheeseScoreEffectPool.Count == 0)
        {
            CreateEffectPools();
        }
    }
    
    /// <summary>
    /// Sets the maximum number of active effects
    /// </summary>
    public void SetMaxActiveEffects(int maxEffects)
    {
        maxActiveEffects = Mathf.Max(1, maxEffects);
    }
    
    /// <summary>
    /// Gets debug information about the VFX system
    /// </summary>
    public string GetDebugInfo()
    {
        return $"VFX Manager: {activeEffects.Count}/{maxActiveEffects} active effects, " +
               $"Pooling: {(enableEffectPooling ? "enabled" : "disabled")}, " +
               $"Pools: CheeseScore({cheeseScoreEffectPool.Count}) Death({deathEffectPool.Count}) " +
               $"Buff({buffEffectPool.Count}) Teleport({teleportEffectPool.Count}) " +
               $"Sopa({sopaEffectPool.Count}) MouseHole({mouseHoleEffectPool.Count})";
    }
    
    /// <summary>
    /// Creates a missing effect prefab automatically
    /// </summary>
    GameObject CreateMissingEffectPrefab(string effectName)
    {
        GameObject effectPrefab = new GameObject($"{effectName}EffectPrefab");
        
        // Add particle system
        ParticleSystem particles = effectPrefab.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 1.0f;
        main.startSpeed = 2.0f;
        main.maxParticles = 20;
        
        // Configure based on effect type
        switch (effectName)
        {
            case "SopaAttack":
                main.startColor = Color.yellow;
                main.startSize = 0.5f;
                main.maxParticles = 50; // More particles for bigger effect
                var shape = particles.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(2f, 2f, 1f); // 2x2 box shape to match attack area
                break;
                
            case "Teleport":
                main.startColor = Color.cyan;
                main.startSize = 0.2f;
                break;
                
            case "CheeseScore":
                main.startColor = Color.green;
                main.startSize = 0.15f;
                break;
                
            case "MouseDeath":
                main.startColor = Color.red;
                main.startSize = 0.25f;
                break;
                
            case "BuffActivation":
                main.startColor = Color.blue;
                main.startSize = 0.2f;
                break;
                
            case "MouseHoleSpawn":
                main.startColor = Color.magenta;
                main.startSize = 0.3f;
                break;
        }
        
        // Add auto-destroy component
        effectPrefab.AddComponent<AutoDestroyParticleEffect>();
        
        Debug.Log($"VFXManager: Auto-created {effectName} effect prefab");
        return effectPrefab;
    }
    
    /// <summary>
    /// Assigns the created prefab to the appropriate field
    /// </summary>
    void AssignCreatedPrefab(string effectName, GameObject prefab)
    {
        switch (effectName)
        {
            case "SopaAttack":
                sopaAttackEffectPrefab = prefab;
                break;
            case "Teleport":
                teleportEffectPrefab = prefab;
                break;
            case "CheeseScore":
                cheeseScoreEffectPrefab = prefab;
                break;
            case "MouseDeath":
                mouseDeathEffectPrefab = prefab;
                break;
            case "BuffActivation":
                buffActivationEffectPrefab = prefab;
                break;
            case "MouseHoleSpawn":
                mouseHoleSpawnEffectPrefab = prefab;
                break;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

/// <summary>
/// Helper component that automatically destroys particle effects when they finish
/// </summary>
public class AutoDestroyParticleEffect : MonoBehaviour
{
    void Start()
    {
        ParticleSystem particles = GetComponent<ParticleSystem>();
        if (particles != null)
        {
            // Destroy the object when the particle system stops playing
            Destroy(gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
        }
        else
        {
            // Fallback: destroy after 2 seconds if no particle system found
            Destroy(gameObject, 2f);
        }
    }
}