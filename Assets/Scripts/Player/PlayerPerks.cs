using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player upgrades and perks purchased from the market
/// </summary>
public class PlayerPerks : MonoBehaviour
{
    [Header("Movement Upgrades")]
    [SerializeField] private bool hasSpeedBoost = false;
    [SerializeField] private float speedMultiplier = 1f;
    
    [Header("Combat Upgrades")]
    [SerializeField] private bool hasSopa = false;
    [SerializeField] private float sopaCooldown = 30f; // Set to 30 seconds
    private float lastSopaTime = -15f;
    
    [Header("Utility Upgrades")]
    [SerializeField] private bool canTeleport = false;
    [SerializeField] private float teleportCooldown = 15f; // Set to 15 seconds as requested
    private float lastTeleportTime = -15f;
    
    [Header("Collection Upgrades")]
    [SerializeField] private bool hasBuffMagnet = false;
    [SerializeField] private float buffMagnetRange = 3f;
    [SerializeField] private bool hasCheeseMultiplier = false;
    [SerializeField] private int cheeseMultiplier = 1;
    
    [Header("Map Upgrades")]
    [SerializeField] private bool hasMapExpansion = false;
    
    [Header("Special Upgrades")]
    [SerializeField] private bool hasInvincibility = false;
    [SerializeField] private bool hasSlowMotion = false;
    
    [Header("Visual Effects")]
    private SpriteRenderer spriteRenderer;
    private Coroutine buffVisualCoroutine;
    
    // Public properties for external access
    public bool HasSpeedBoost => hasSpeedBoost;
    public float SpeedMultiplier => speedMultiplier;
    public bool HasSopa => hasSopa;
    public bool CanTeleport => canTeleport;
    public bool HasBuffMagnet => hasBuffMagnet;
    public float BuffMagnetRange => buffMagnetRange;
    public bool HasCheeseMultiplier => hasCheeseMultiplier;
    public int CheeseMultiplier => cheeseMultiplier;
    public bool HasMapExpansion => hasMapExpansion;
    public bool HasInvincibility => hasInvincibility;
    public bool HasSlowMotion => hasSlowMotion;
    
    // Cooldown properties for UI
    public float SopaCooldown => sopaCooldown;
    public float TeleportCooldown => teleportCooldown;
    public float LastSopaTime => lastSopaTime;
    public float LastTeleportTime => lastTeleportTime;
    
    // Temporary speed boost variables
    private float originalSpeedMultiplier = 1f;
    private bool hasTemporarySpeedBoost = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("PlayerPerks: SpriteRenderer not found! Visual effects won't work.");
        }
    }
    
    void Update()
    {
        // Input handling moved to PlayerControllerNewInput to avoid conflicts
        // This Update method can be used for other periodic checks if needed
    }
    
    public void UseSopa()
    {
        // Only use sopa if player has the ability
        if (!hasSopa)
        {
            Debug.LogWarning("PlayerPerks: Attempted to use Sopa without having the ability!");
            return;
        }
        
        // Check cooldown
        if (Time.time - lastSopaTime < sopaCooldown)
        {

            return;
        }
        
        lastSopaTime = Time.time;
        
        // Find ALL cats in the scene and destroy them
        Cat[] allCats = FindObjectsByType<Cat>(FindObjectsSortMode.None);
        int catsDestroyed = 0;
        
        foreach (Cat cat in allCats)
        {
            if (cat != null)
            {
                // Play individual death effect for each cat
                VFXManager.Instance?.PlayDeathEffect(cat.transform.position);
                Destroy(cat.gameObject);
                catsDestroyed++;
            }
        }
        
        // Play main sopa effect at player position
        VFXManager.Instance?.PlaySopaAttackEffect(transform.position);
        AudioManager.Instance?.PlaySopaAttack();
        

    }
    
    public void UseTeleport()
    {
        // Only use teleport if player has the ability
        if (!canTeleport)
        {
            Debug.LogWarning("PlayerPerks: Attempted to use Teleport without having the ability!");
            return;
        }
        
        // Check cooldown
        if (Time.time - lastTeleportTime < teleportCooldown)
        {

            return;
        }
        
        lastTeleportTime = Time.time;
        
        // Store current position for effect
        Vector3 oldPosition = transform.position;
        
        // Get mouse position in world coordinates with proper camera handling
        Vector3 mousePos = GetMouseWorldPosition();
        
        // Validate teleport position (check if it's within bounds)
        if (IsValidTeleportPosition(mousePos))
        {
            // Teleport to mouse position
            transform.position = mousePos;
            
            // Play teleport effect ONLY at old position (where player disappeared from)
            VFXManager.Instance?.PlayTeleportEffect(oldPosition); // Where player disappeared from
            AudioManager.Instance?.PlayTeleport();
            

        }
        else
        {
            Debug.LogWarning($"PlayerPerks: Invalid teleport position {mousePos}, teleport cancelled");
            // Refund the cooldown since teleport failed
            lastTeleportTime = Time.time - teleportCooldown;
        }
    }
    
    /// <summary>
    /// Gets mouse position in world coordinates with proper camera handling
    /// </summary>
    Vector3 GetMouseWorldPosition()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("PlayerPerks: Main camera not found for teleport!");
            return transform.position; // Fallback to current position
        }
        
        // Use Mouse.current from Input System for more accurate position
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, camera.nearClipPlane));
        mouseWorldPos.z = 0f; // Ensure Z is 0 for 2D
        

        return mouseWorldPos;
    }
    
    /// <summary>
    /// Validates if the teleport position is safe and within bounds
    /// </summary>
    bool IsValidTeleportPosition(Vector3 position)
    {
        // Check if position is within camera bounds (expanded area)
        Camera camera = Camera.main;
        if (camera != null)
        {
            Vector3 viewportPos = camera.WorldToViewportPoint(position);
            
            // Allow teleport within expanded viewport (10% margin outside visible area)
            if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || 
                viewportPos.y < -0.1f || viewportPos.y > 1.1f)
            {
                Debug.LogWarning($"PlayerPerks: Teleport position {position} is outside camera bounds");
                return false;
            }
        }
        
        // Check for wall collisions using raycast
        LayerMask wallLayerMask = 1 << 6; // Walls layer
        Collider2D wallCollision = Physics2D.OverlapCircle(position, 0.5f, wallLayerMask);
        if (wallCollision != null)
        {
            Debug.LogWarning($"PlayerPerks: Teleport position {position} is inside a wall");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Activates an upgrade based on its type
    /// </summary>
    public void ActivateUpgrade(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.SpeedBoost:
                hasSpeedBoost = true;
                speedMultiplier = 1.5f;

                break;
                
            case UpgradeType.Sopa:
                hasSopa = true;

                break;
                
            case UpgradeType.Teleport:
                canTeleport = true;

                break;
                
            case UpgradeType.BuffMagnet:
                hasBuffMagnet = true;

                break;
                
            case UpgradeType.MapExpansion:
                hasMapExpansion = true;
                ExpandMap();

                break;
                
            case UpgradeType.CheeseMultiplier:
                hasCheeseMultiplier = true;
                cheeseMultiplier = 2;

                break;
                
            case UpgradeType.Invincibility:
                hasInvincibility = true;

                break;
                
            case UpgradeType.SlowMotion:
                hasSlowMotion = true;

                break;
        }
        
        // Start visual effect for upgrade activation
        StartBuffVisualEffect(3f); // 3 seconds of blinking
        
        // Trigger global difficulty update
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateCatAIDifficulty();
        }
    }
    
    /// <summary>
    /// Checks if player has a specific upgrade
    /// </summary>
    public bool HasUpgrade(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.SpeedBoost: return hasSpeedBoost;
            case UpgradeType.Sopa: return hasSopa;
            case UpgradeType.Teleport: return canTeleport;
            case UpgradeType.BuffMagnet: return hasBuffMagnet;
            case UpgradeType.MapExpansion: return hasMapExpansion;
            case UpgradeType.CheeseMultiplier: return hasCheeseMultiplier;
            case UpgradeType.Invincibility: return hasInvincibility;
            case UpgradeType.SlowMotion: return hasSlowMotion;
            default: return false;
        }
    }
    
    /// <summary>
    /// Gets the number of active upgrades
    /// </summary>
    public int GetActiveUpgradeCount()
    {
        int count = 0;
        if (hasSpeedBoost) count++;
        if (hasSopa) count++;
        if (canTeleport) count++;
        if (hasBuffMagnet) count++;
        if (hasMapExpansion) count++;
        if (hasCheeseMultiplier) count++;
        if (hasInvincibility) count++;
        if (hasSlowMotion) count++;
        return count;
    }
    
    /// <summary>
    /// Resets all upgrades (for new game)
    /// </summary>
    public void ResetUpgrades()
    {
        hasSpeedBoost = false;
        speedMultiplier = 1f;
        hasSopa = false;
        canTeleport = false;
        hasBuffMagnet = false;
        hasCheeseMultiplier = false;
        cheeseMultiplier = 1;
        hasMapExpansion = false;
        hasInvincibility = false;
        hasSlowMotion = false;
        
        lastSopaTime = -30f;
        lastTeleportTime = -15f;
        

    }
    
    /// <summary>
    /// Starts a temporary buff visual effect (for invincibility, upgrades, etc.)
    /// </summary>
    public void StartBuffVisualEffect(float duration)
    {
        if (spriteRenderer == null) 
        {
            Debug.LogWarning("PlayerPerks: SpriteRenderer is null, cannot start visual effect!");
            return;
        }
        
        // Stop any existing visual effect
        if (buffVisualCoroutine != null)
        {
            StopCoroutine(buffVisualCoroutine);
        }
        

        buffVisualCoroutine = StartCoroutine(BuffVisualCoroutine(duration));
    }
    
    /// <summary>
    /// Coroutine that handles the blinking/glowing effect
    /// </summary>
    System.Collections.IEnumerator BuffVisualCoroutine(float duration)
    {
        if (spriteRenderer == null) 
        {
            Debug.LogWarning("PlayerPerks: SpriteRenderer is null in coroutine!");
            yield break;
        }
        
        Color originalColor = spriteRenderer.color;
        float elapsed = 0f;
        float blinkInterval = 0.15f; // Her 0.15 saniyede bir yanıp sön
        

        
        while (elapsed < duration)
        {
            // Görünür yap
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkInterval);
            
            // Görünmez yap (alpha = 0.3 ile yarı saydam)
            Color transparentColor = originalColor;
            transparentColor.a = 0.3f;
            spriteRenderer.color = transparentColor;
            yield return new WaitForSeconds(blinkInterval);
            
            elapsed += (blinkInterval * 2f);
        }
        
        // Restore original color
        spriteRenderer.color = originalColor;
        buffVisualCoroutine = null;

    }
    
    /// <summary>
    /// Stops any active buff visual effect
    /// </summary>
    public void StopBuffVisualEffect()
    {
        if (buffVisualCoroutine != null)
        {
            StopCoroutine(buffVisualCoroutine);
            buffVisualCoroutine = null;
        }
        
        if (spriteRenderer != null)
        {
            // Restore original color
            spriteRenderer.color = Color.white;
        }
    }
    
    /// <summary>
    /// Expands the map by increasing camera size and updating all spawners
    /// </summary>
    void ExpandMap()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            float expansionMultiplier = 1.5f; // 50% büyütme
            
            if (mainCamera.orthographic)
            {
                mainCamera.orthographicSize *= expansionMultiplier;
            }
            else
            {
                mainCamera.fieldOfView *= 1.3f; // Perspective kamera için
            }
            
            // Update all spawners to use the expanded area
            UpdateSpawnersForMapExpansion(expansionMultiplier);
            

        }
    }
    
    /// <summary>
    /// Updates all spawners to use the expanded map area
    /// </summary>
    void UpdateSpawnersForMapExpansion(float expansionMultiplier)
    {
        // Find and update all spawners
        BaseSpawner[] allSpawners = FindObjectsByType<BaseSpawner>(FindObjectsSortMode.None);
        
        foreach (BaseSpawner spawner in allSpawners)
        {
            float currentRadius = spawner.GetSpawnRadius();
            float newRadius = currentRadius * expansionMultiplier;
            spawner.UpdateSpawnRadius(newRadius);
            

        }
        
        // Also expand walls if they exist
        ExpandWallsForMapExpansion(expansionMultiplier);
        

    }
    
    /// <summary>
    /// Expands walls to match the new map size
    /// </summary>
    void ExpandWallsForMapExpansion(float expansionMultiplier)
    {
        // Find walls by layer (layer 6) or tag
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Ground"); // Walls might be tagged as Ground
        
        if (walls.Length == 0)
        {
            // Try finding by layer
            Collider2D[] wallColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            foreach (var collider in wallColliders)
            {
                if (collider.gameObject.layer == 6) // Walls layer
                {
                    System.Array.Resize(ref walls, walls.Length + 1);
                    walls[walls.Length - 1] = collider.gameObject;
                }
            }
        }
        
        foreach (GameObject wall in walls)
        {
            // Scale wall transforms
            Vector3 currentScale = wall.transform.localScale;
            Vector3 currentPosition = wall.transform.position;
            
            // Scale the wall size
            wall.transform.localScale = currentScale * expansionMultiplier;
            
            // Move walls outward to maintain proper boundaries
            if (Mathf.Abs(currentPosition.x) > Mathf.Abs(currentPosition.y)) // Horizontal walls
            {
                wall.transform.position = new Vector3(
                    currentPosition.x * expansionMultiplier, 
                    currentPosition.y, 
                    currentPosition.z
                );
            }
            else // Vertical walls
            {
                wall.transform.position = new Vector3(
                    currentPosition.x, 
                    currentPosition.y * expansionMultiplier, 
                    currentPosition.z
                );
            }
            

        }
        

    }
    
    /// <summary>
    /// Sets temporary speed multiplier for buffs
    /// </summary>
    public void SetTemporarySpeedMultiplier(float multiplier)
    {
        if (!hasTemporarySpeedBoost)
        {
            originalSpeedMultiplier = speedMultiplier;
            hasTemporarySpeedBoost = true;
        }
        speedMultiplier = multiplier;

    }
    
    /// <summary>
    /// Restores original speed multiplier
    /// </summary>
    public void RestoreOriginalSpeedMultiplier()
    {
        if (hasTemporarySpeedBoost)
        {
            speedMultiplier = originalSpeedMultiplier;
            hasTemporarySpeedBoost = false;

        }
    }
    
    /// <summary>
    /// Gets debug information about current perks
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Speed: {speedMultiplier:F1}x, Sopa: {hasSopa}, Teleport: {canTeleport}, " +
               $"BuffMagnet: {hasBuffMagnet}, CheeseMulti: {cheeseMultiplier}x, MapExpansion: {hasMapExpansion}";
    }
}