using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a collectible buff that provides temporary effects to the player
/// Handles collection detection, effect activation, and visual feedback
/// </summary>
public class BuffCollectible : MonoBehaviour
{
    [Header("Buff Configuration")]
    [SerializeField] private BuffType buffType = BuffType.Invincibility;
    [SerializeField] private float effectDuration = 5f;
    [SerializeField] private float despawnTime = 20f; // Auto-despawn if not collected
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.3f;
    
    [Header("Magnet Effect")]
    [SerializeField] private bool magnetEnabled = false;
    [SerializeField] private float magnetRange = 5f;
    [SerializeField] private float magnetStrength = 3f;
    
    // Static variables to manage Time.timeScale safely
    private static bool isSlowMotionActive = false;
    private static Coroutine activeSlowMotionCoroutine = null;
    private static MonoBehaviour slowMotionHost = null;
    
    private float spawnTime;
    private Transform playerTransform;
    private bool isCollected = false;
    private Vector3 originalScale;
    
    void Start()
    {
        spawnTime = Time.time;
        originalScale = transform.localScale;
        
        // Get components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
            
        // Ensure trigger is set
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
            
        // Find player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Start auto-despawn timer
        StartCoroutine(AutoDespawnTimer());
        
        Debug.Log($"BuffCollectible: {buffType} buff spawned");
    }
    
    void Update()
    {
        // Visual pulsing effect
        UpdateVisualEffects();
        
        // Magnet effect
        if (magnetEnabled && playerTransform != null && !isCollected)
        {
            UpdateMagnetEffect();
        }
    }
    
    /// <summary>
    /// Updates visual pulsing effects
    /// </summary>
    void UpdateVisualEffects()
    {
        if (spriteRenderer == null) return;
        
        // Pulsing scale effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
        transform.localScale = originalScale * pulse;
        
        // Color pulsing based on buff type
        Color baseColor = GetBuffColor();
        float colorPulse = 0.7f + Mathf.Sin(Time.time * pulseSpeed * 1.5f) * 0.3f;
        spriteRenderer.color = baseColor * colorPulse;
    }
    
    /// <summary>
    /// Updates magnet effect to attract player
    /// </summary>
    void UpdateMagnetEffect()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= magnetRange)
        {
            // Move towards player
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float magnetForce = magnetStrength * (1f - distanceToPlayer / magnetRange);
            transform.position += direction * magnetForce * Time.deltaTime;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectBuff(other.gameObject);
        }
    }
    
    /// <summary>
    /// Handles buff collection and effect activation
    /// </summary>
    void CollectBuff(GameObject player)
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Apply buff effect
        ApplyBuffEffect(player);
        
        // Visual/audio feedback
        PlayCollectionEffects();
        
        // Destroy the buff object
        Destroy(gameObject);
        
        Debug.Log($"BuffCollectible: {buffType} buff collected by player");
    }
    
    /// <summary>
    /// Applies the buff effect to the player
    /// </summary>
    void ApplyBuffEffect(GameObject player)
    {
        switch (buffType)
        {
            case BuffType.Invincibility:
                ApplyInvincibilityBuff(player);
                break;
                
            case BuffType.SpeedBoost:
                ApplySpeedBoostBuff(player);
                break;
                
            case BuffType.SlowMotion:
                ApplySlowMotionBuff(player);
                break;
                
            case BuffType.CheeseMultiplier:
                ApplyCheeseMultiplierBuff(player);
                break;
                
            default:
                Debug.LogWarning($"BuffCollectible: Unknown buff type {buffType}");
                break;
        }
    }
    
    /// <summary>
    /// Applies invincibility buff effect
    /// </summary>
    void ApplyInvincibilityBuff(GameObject player)
    {
        PlayerControllerNewInput playerController = player.GetComponent<PlayerControllerNewInput>();
        if (playerController != null)
        {
            playerController.ActivateInvincibility(effectDuration);
            Debug.Log($"BuffCollectible: Applied invincibility for {effectDuration} seconds");
        }
        else
        {
            Debug.LogWarning("BuffCollectible: PlayerControllerNewInput component not found on player!");
        }
    }
    
    /// <summary>
    /// Applies speed boost buff effect
    /// </summary>
void ApplySpeedBoostBuff(GameObject player)
    {
        PlayerPerks playerPerks = player.GetComponent<PlayerPerks>();
        if (playerPerks != null)
        {
            // Visual efekt ekle
            playerPerks.StartBuffVisualEffect(effectDuration);
            
            // GameManager üzerinden speed boost başlat (buff object destroy olsa bile devam eder)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartCoroutine(TemporarySpeedBoost(playerPerks));
                
                // Güvenlik timer'ı da ekle
                GameManager.Instance.StartCoroutine(SpeedBoostSafetyTimer(playerPerks));
            }
            else
            {
                // Fallback: player üzerinden başlat
                MonoBehaviour playerMono = player.GetComponent<MonoBehaviour>();
                if (playerMono != null)
                {
                    playerMono.StartCoroutine(TemporarySpeedBoost(playerPerks));
                    
                    // Güvenlik timer'ı da ekle
                    playerMono.StartCoroutine(SpeedBoostSafetyTimer(playerPerks));
                }
            }
            
            Debug.Log($"BuffCollectible: Applied speed boost for {effectDuration} seconds with visual effect");
        }
    }
    
    /// <summary>
    /// Applies slow motion buff effect
    /// </summary>
void ApplySlowMotionBuff(GameObject player)
    {
        PlayerPerks playerPerks = player.GetComponent<PlayerPerks>();
        if (playerPerks != null)
        {
            // Visual efekt ekle
            playerPerks.StartBuffVisualEffect(effectDuration);
        }
        
        // Eğer zaten slow motion aktifse, mevcut olanı iptal et
        if (isSlowMotionActive && activeSlowMotionCoroutine != null && slowMotionHost != null)
        {
            slowMotionHost.StopCoroutine(activeSlowMotionCoroutine);
            Debug.Log("BuffCollectible: Cancelled previous slow motion effect");
        }
        
        // GameManager üzerinden slow motion başlat (buff object destroy olsa bile devam eder)
        if (GameManager.Instance != null)
        {
            activeSlowMotionCoroutine = GameManager.Instance.StartCoroutine(TemporarySlowMotion());
            slowMotionHost = GameManager.Instance;
            
            // Güvenlik timer'ı da ekle
            GameManager.Instance.StartCoroutine(SlowMotionSafetyTimer());
        }
        else
        {
            // Fallback: player üzerinden başlat
            MonoBehaviour playerMono = player.GetComponent<MonoBehaviour>();
            if (playerMono != null)
            {
                activeSlowMotionCoroutine = playerMono.StartCoroutine(TemporarySlowMotion());
                slowMotionHost = playerMono;
                
                // Güvenlik timer'ı da ekle
                playerMono.StartCoroutine(SlowMotionSafetyTimer());
            }
        }
        
        Debug.Log($"BuffCollectible: Applied slow motion for {effectDuration} seconds with visual effect");
    }
    
    /// <summary>
    /// Applies cheese multiplier buff effect
    /// </summary>
void ApplyCheeseMultiplierBuff(GameObject player)
    {
        PlayerPerks playerPerks = player.GetComponent<PlayerPerks>();
        if (playerPerks != null)
        {
            // Visual efekt ekle
            playerPerks.StartBuffVisualEffect(effectDuration);
        }
        
        // This would need integration with GameManager for cheese collection multiplier
        Debug.Log($"BuffCollectible: Applied cheese multiplier for {effectDuration} seconds with visual effect");
        // TODO: Implement cheese multiplier effect
    }
    
    /// <summary>
    /// Coroutine for temporary speed boost effect
    /// </summary>
    IEnumerator TemporarySpeedBoost(PlayerPerks playerPerks)
    {
        if (playerPerks == null) yield break;
        
        float originalMultiplier = playerPerks.SpeedMultiplier;
        float boostMultiplier = originalMultiplier * 1.5f; // 50% speed increase
        
        Debug.Log($"BuffCollectible: Speed boost started - Original: {originalMultiplier:F1}x, Boosted: {boostMultiplier:F1}x for {effectDuration} seconds");
        
        // Apply boost
        playerPerks.SetTemporarySpeedMultiplier(boostMultiplier);
        
        try
        {
            yield return new WaitForSeconds(effectDuration);
        }
        finally
        {
            // Restore original speed
            if (playerPerks != null)
            {
                playerPerks.RestoreOriginalSpeedMultiplier();
                Debug.Log("BuffCollectible: Speed boost effect ended - speed restored");
            }
        }
    }
    
    /// <summary>
    /// Güvenlik timer'ı - ana speed boost coroutine başarısız olursa speed'i geri yükler
    /// </summary>
    IEnumerator SpeedBoostSafetyTimer(PlayerPerks playerPerks)
    {
        float safetyDuration = effectDuration + 1f; // 1 saniye ekstra buffer
        yield return new WaitForSeconds(safetyDuration);
        
        // Eğer player hala varsa ve geçici speed boost aktifse, zorla geri yükle
        if (playerPerks != null)
        {
            Debug.LogWarning("BuffCollectible: Speed boost safety timer triggered! Force-restoring speed");
            playerPerks.RestoreOriginalSpeedMultiplier();
        }
    }
    
    /// <summary>
    /// Safety timer to ensure time scale is restored even if main coroutine fails
    /// </summary>
    IEnumerator SlowMotionSafetyTimer()
    {
        float safetyDuration = effectDuration + 1f; // Extra 1 second buffer
        yield return new WaitForSecondsRealtime(safetyDuration);
        
        // If slow motion is still active after the safety duration, force restore
        if (isSlowMotionActive || Time.timeScale < 0.9f)
        {
            Debug.LogWarning("BuffCollectible: Safety timer triggered! Force-restoring TimeScale to 1.0");
            Time.timeScale = 1f;
            isSlowMotionActive = false;
            activeSlowMotionCoroutine = null;
            slowMotionHost = null;
        }
    }
    

    
    /// <summary>
    /// Coroutine for temporary slow motion effect
    /// </summary>
    IEnumerator TemporarySlowMotion()
    {
        // Mark slow motion as active
        isSlowMotionActive = true;
        
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.5f; // Half speed
        
        Debug.Log($"BuffCollectible: Slow motion started - TimeScale: {Time.timeScale} for {effectDuration} seconds (Original: {originalTimeScale})");
        
        try
        {
            yield return new WaitForSecondsRealtime(effectDuration); // Use real time since we changed time scale
        }
        finally
        {
            // Ensure we ALWAYS restore the original time scale, even if something goes wrong
            Time.timeScale = originalTimeScale;
            
            // Mark slow motion as inactive
            isSlowMotionActive = false;
            activeSlowMotionCoroutine = null;
            slowMotionHost = null;
            
            Debug.Log($"BuffCollectible: Slow motion effect ended - TimeScale restored to: {Time.timeScale}");
            
            // Double-check that time scale is properly restored
            if (Mathf.Abs(Time.timeScale - originalTimeScale) > 0.01f)
            {
                Debug.LogWarning($"BuffCollectible: TimeScale mismatch detected! Expected: {originalTimeScale}, Actual: {Time.timeScale}. Force-correcting...");
                Time.timeScale = originalTimeScale;
            }
        }
    }
    
    /// <summary>
    /// Auto-despawn timer coroutine
    /// </summary>
    IEnumerator AutoDespawnTimer()
    {
        yield return new WaitForSeconds(despawnTime);
        
        if (!isCollected)
        {
            Debug.Log($"BuffCollectible: {buffType} buff auto-despawned");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Plays collection effects
    /// </summary>
    void PlayCollectionEffects()
    {
        // Play buff collection sound
        AudioManager.Instance?.PlayBuffCollection();
        
        // Play buff activation visual effects
        VFXManager.Instance?.PlayBuffActivationEffect(transform.position);
    }
    
    /// <summary>
    /// Gets the color associated with this buff type
    /// </summary>
    Color GetBuffColor()
    {
        switch (buffType)
        {
            case BuffType.Invincibility: return Color.yellow;
            case BuffType.SpeedBoost: return Color.green;
            case BuffType.SlowMotion: return Color.blue;
            case BuffType.CheeseMultiplier: return Color.magenta;
            default: return Color.white;
        }
    }
    
    /// <summary>
    /// Enables magnet effect for this buff
    /// </summary>
    public void EnableMagnetEffect(float range)
    {
        magnetEnabled = true;
        magnetRange = range;
    }
    
    /// <summary>
    /// Gets the buff type
    /// </summary>
    public BuffType GetBuffType()
    {
        return buffType;
    }
    
    /// <summary>
    /// Gets the effect duration
    /// </summary>
    public float GetEffectDuration()
    {
        return effectDuration;
    }
}

/// <summary>
/// Types of buffs available in the game
/// </summary>
public enum BuffType
{
    Invincibility,      // Makes player invincible to cats
    SpeedBoost,         // Increases player movement speed
    SlowMotion,         // Slows down time for easier navigation
    CheeseMultiplier    // Multiplies cheese collection for duration
}