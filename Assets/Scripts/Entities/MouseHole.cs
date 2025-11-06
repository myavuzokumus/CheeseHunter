using UnityEngine;
using System.Collections;

/// <summary>
/// Mouse hole that appears when player reaches high scores
/// Provides the "push your luck" decision: continue playing for higher scores or secure current progress
/// </summary>
public class MouseHole : MonoBehaviour
{
    [Header("Hole Configuration")]
    [SerializeField] private int requiredCheese = 100; // Minimum cheese needed to spawn
    [SerializeField] private float despawnTime = 60f; // Auto-despawn after 60 seconds if not used
    [SerializeField] private bool isPermanent = false; // If true, hole doesn't despawn
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseIntensity = 0.2f;
    
    [Header("Warning System")]
    [SerializeField] private bool showWarning = true;
    [SerializeField] private float warningDuration = 3f;
    
    private float spawnTime;
    private bool isPlayerNear = false;
    private bool hasBeenUsed = false;
    private Vector3 originalScale;
    
void Start()
    {
        spawnTime = Time.time;
        originalScale = transform.localScale;
        
        // Ensure correct tag
        if (!gameObject.CompareTag("MouseHole"))
        {
            gameObject.tag = "MouseHole";
            Debug.Log("MouseHole: Set tag to MouseHole");
        }
        
        // Get components
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
            
        // Ensure trigger is set and collider is large enough
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            
            // Make sure collider is big enough for interaction
            if (triggerCollider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = Mathf.Max(circleCollider.radius, 1.5f); // Minimum 1.5 radius
                Debug.Log($"MouseHole: CircleCollider2D radius set to {circleCollider.radius}");
            }
            else if (triggerCollider is BoxCollider2D boxCollider)
            {
                boxCollider.size = Vector2.Max(boxCollider.size, new Vector2(2f, 2f)); // Minimum 2x2 size
                Debug.Log($"MouseHole: BoxCollider2D size set to {boxCollider.size}");
            }
        }
        else
        {
            // Create a collider if none exists
            CircleCollider2D newCollider = gameObject.AddComponent<CircleCollider2D>();
            newCollider.isTrigger = true;
            newCollider.radius = 1.5f;
            triggerCollider = newCollider;
            Debug.Log("MouseHole: Created new CircleCollider2D");
        }
            
        // Visual appearance will be handled by TextureManager
        if (TextureManager.Instance != null && spriteRenderer != null)
        {
            TextureManager.Instance.ApplyTextureToNewObject(gameObject, "mousehole");
        }
        
        // Show warning if enabled
        if (showWarning)
        {
            StartCoroutine(ShowSpawnWarning());
        }
        
        // Start auto-despawn timer if not permanent
        if (!isPermanent)
        {
            StartCoroutine(AutoDespawnTimer());
        }
        
        Debug.Log($"MouseHole: Spawned at {transform.position} (Required cheese: {requiredCheese}, Tag: {gameObject.tag}, Trigger: {triggerCollider != null}, Radius: {(triggerCollider as CircleCollider2D)?.radius})");
    }
    
    void Update()
    {
        // Visual pulsing effect
        UpdateVisualEffects();
        
        // Auto-despawn check based on cheese requirement
        CheckDespawnConditions();
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
        
        // Simple alpha pulsing - no color changes
        if (isPlayerNear)
        {
            float alphaPulse = 0.8f + Mathf.Sin(Time.time * pulseSpeed * 2f) * 0.2f;
            Color currentColor = spriteRenderer.color;
            currentColor.a = alphaPulse;
            spriteRenderer.color = currentColor;
        }
    }
    
    /// <summary>
    /// Checks if hole should despawn due to insufficient cheese
    /// </summary>
    void CheckDespawnConditions()
    {
        if (GameManager.Instance == null) return;
        
        int currentCheese = GameManager.Instance.GetCurrentCheese();
        
        // If player's cheese drops below requirement, despawn the hole
        if (currentCheese < requiredCheese && !hasBeenUsed)
        {
            Debug.Log($"MouseHole: Player cheese ({currentCheese}) below requirement ({requiredCheese}), despawning");
            DespawnHole();
        }
    }
    
void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"MouseHole: Trigger entered by {other.name} (Tag: {other.tag}, Position: {other.transform.position})");
        
        if ((other.CompareTag("Player") || other.name.ToLower().Contains("player") || other.name.ToLower().Contains("mouse")) && !hasBeenUsed)
        {
            isPlayerNear = true;
            Debug.Log("MouseHole: Player detected near hole! Triggering win immediately!");
            
            // Trigger win immediately - don't check cheese requirement since player already qualified
            TriggerWinCondition();
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
    
    /// <summary>
    /// Triggers the win condition when player enters the hole
    /// </summary>
    void TriggerWinCondition()
    {
        if (hasBeenUsed) return;
        
        hasBeenUsed = true;
        
        Debug.Log("MouseHole: Player entered hole - triggering win condition!");
        
        // Play win effects
        PlayWinEffects();
        
        // Trigger game manager win condition
        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinGame();
        }
        
        // Keep the hole visible for the win screen
        // Don't destroy it immediately
    }
    
    /// <summary>
    /// Shows spawn warning effect
    /// </summary>
    IEnumerator ShowSpawnWarning()
    {
        // Simple visual warning - flash the sprite
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            
            for (float t = 0; t < warningDuration; t += 0.1f)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.white, Mathf.PingPong(t * 4f, 1f));
                yield return new WaitForSeconds(0.1f);
            }
            
            spriteRenderer.color = originalColor;
        }
        
        Debug.Log("MouseHole: Spawn warning completed");
    }
    
    /// <summary>
    /// Auto-despawn timer coroutine
    /// </summary>
    IEnumerator AutoDespawnTimer()
    {
        yield return new WaitForSeconds(despawnTime);
        
        if (!hasBeenUsed)
        {
            Debug.Log("MouseHole: Auto-despawning after timeout");
            DespawnHole();
        }
    }
    
    /// <summary>
    /// Plays win effects when player enters hole
    /// </summary>
    void PlayWinEffects()
    {
        // Play victory sound
        AudioManager.Instance?.PlayVictory();
        
        // TODO: Add particle effects
        // VFXManager.Instance?.PlayMouseHoleWinEffect(transform.position);
        
        // Simple visual feedback for now
        if (spriteRenderer != null)
        {
            StartCoroutine(WinVisualEffect());
        }
    }
    
    /// <summary>
    /// Simple win visual effect
    /// </summary>
    IEnumerator WinVisualEffect()
    {
        Color originalColor = spriteRenderer.color;
        
        // Flash bright colors
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
        
        spriteRenderer.color = originalColor;
    }
    
    /// <summary>
    /// Despawns the mouse hole
    /// </summary>
    void DespawnHole()
    {
        Debug.Log("MouseHole: Despawning");
        
        // TODO: Play despawn effect
        // VFXManager.Instance?.PlayMouseHoleDespawnEffect(transform.position);
        
        // Notify GameManager that hole is gone
        if (GameManager.Instance != null)
        {
            MouseHoleSpawner spawner = GameManager.Instance.GetMouseHoleSpawner();
            if (spawner != null)
            {
                spawner.OnHoleDespawned();
            }
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Sets the required cheese amount
    /// </summary>
    public void SetRequiredCheese(int amount)
    {
        requiredCheese = amount;
    }
    
    /// <summary>
    /// Sets whether the hole is permanent
    /// </summary>
    public void SetPermanent(bool permanent)
    {
        isPermanent = permanent;
    }
    
    /// <summary>
    /// Gets the required cheese amount
    /// </summary>
    public int GetRequiredCheese()
    {
        return requiredCheese;
    }
    
    /// <summary>
    /// Checks if the hole has been used
    /// </summary>
    public bool HasBeenUsed()
    {
        return hasBeenUsed;
    }
    
    /// <summary>
    /// Gets time remaining before auto-despawn
    /// </summary>
    public float GetTimeRemaining()
    {
        if (isPermanent) return float.MaxValue;
        return Mathf.Max(0f, despawnTime - (Time.time - spawnTime));
    }
}