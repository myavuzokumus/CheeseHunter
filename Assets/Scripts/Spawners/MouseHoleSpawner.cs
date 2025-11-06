using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages spawning of mouse holes based on player progress
/// Implements the "push your luck" mechanic by offering escape routes at high scores
/// </summary>
public class MouseHoleSpawner : BaseSpawner
{
    [Header("Spawn Configuration")]
    [SerializeField] private GameObject mouseHolePrefab;
    [SerializeField] private int[] cheeseThresholds = { 50, 100, 200, 300 }; // Daha uygun threshold'lar // When to spawn holes
    [SerializeField] private bool allowMultipleHoles = false; // Can multiple holes exist?
    
    [Header("Progressive Difficulty")]
    [SerializeField] private bool increaseRequirement = true; // Increase cheese requirement over time
    [SerializeField] private float requirementMultiplier = 1.2f; // Multiplier for each subsequent hole
    
    private HashSet<int> spawnedThresholds = new HashSet<int>();
    private List<MouseHole> activeHoles = new List<MouseHole>();
    private int currentThresholdIndex = 0;
    
    protected override void Start()
    {
        base.Start(); // BaseSpawner'dan player referansını al
        
        // Set spawn radius to 10 for mouse holes
        spawnRadius = 10f;
        minDistanceFromPlayer = 4f; // Mouse holes need more distance from player
        
        // Auto-setup prefab if null
        if (mouseHolePrefab == null)
        {
            GameObject holePrefab = GameObject.Find("MouseHolePrefab");
            if (holePrefab != null)
            {
                mouseHolePrefab = holePrefab;

            }
            else
            {
                Debug.LogError("MouseHoleSpawner: Mouse hole prefab not assigned and MouseHolePrefab not found!");
            }
        }
        

    }
    
    void Update()
    {
        CheckForHoleSpawn();
        CleanupDestroyedHoles();
    }
    
    /// <summary>
    /// Checks if a mouse hole should be spawned based on current cheese amount
    /// </summary>
    void CheckForHoleSpawn()
    {
        if (GameManager.Instance == null) return;
        
        int currentCheese = GameManager.Instance.GetCurrentCheese();
        
        // Check each threshold
        for (int i = 0; i < cheeseThresholds.Length; i++)
        {
            int threshold = cheeseThresholds[i];
            
            // Apply progressive difficulty if enabled
            if (increaseRequirement && i > 0)
            {
                threshold = Mathf.RoundToInt(cheeseThresholds[i] * Mathf.Pow(requirementMultiplier, i));
            }
            
            if (currentCheese >= threshold && !spawnedThresholds.Contains(threshold))
            {
                // Check if we can spawn more holes
                if (allowMultipleHoles || activeHoles.Count == 0)
                {
                    SpawnMouseHole(threshold);
                    spawnedThresholds.Add(threshold);
                    currentThresholdIndex = i;

                    break; // Only spawn one hole at a time
                }
            }
        }
    }
    
    /// <summary>
    /// Spawns a mouse hole at a valid location
    /// </summary>
void SpawnMouseHole(int requiredCheese)
    {
        Vector3 spawnPosition = FindValidSpawnPosition();
        GameObject holeObject;
        
        // Use prefab if available, otherwise create programmatically
        if (mouseHolePrefab != null)
        {
            holeObject = Instantiate(mouseHolePrefab, spawnPosition, Quaternion.identity);
            
            // Eğer prefab'da MouseHole component'i yoksa ekle
            MouseHole holeComponent = holeObject.GetComponent<MouseHole>();
            if (holeComponent == null)
            {
                holeComponent = holeObject.AddComponent<MouseHole>();

            }
        }
        else
        {
            Debug.LogWarning("MouseHoleSpawner: No prefab assigned, creating simple hole programmatically");
            holeObject = CreateSimpleMouseHole(spawnPosition);
        }
        
        MouseHole finalHoleComponent = holeObject.GetComponent<MouseHole>();
        
        if (finalHoleComponent != null)
        {
            // Configure the hole
            finalHoleComponent.SetRequiredCheese(requiredCheese);
            
            // Make later holes permanent (higher stakes)
            if (currentThresholdIndex >= 2)
            {
                finalHoleComponent.SetPermanent(true);
            }
            
            // Register the hole
            activeHoles.Add(finalHoleComponent);
            

            
            // Show notification to player
            ShowHoleSpawnNotification(requiredCheese);
        }
        else
        {
            Debug.LogError("MouseHoleSpawner: Failed to get or add MouseHole component!");
        }
        
        // Play spawn sound effect
        AudioManager.Instance?.PlayMouseHoleSpawn();
        
        // Play spawn visual effect
        VFXManager.Instance?.PlayMouseHoleSpawnEffect(spawnPosition);
    }
    

    
    /// <summary>
    /// Override to add mouse hole specific distance checks
    /// </summary>
    protected override bool IsValidDistanceFromOthers(Vector3 position)
    {
        // Check distance from existing holes first
        foreach (MouseHole hole in activeHoles)
        {
            if (hole != null)
            {
                float distanceFromHole = Vector3.Distance(position, hole.transform.position);
                if (distanceFromHole < 3f) return false; // Minimum distance between holes
            }
        }
        
        // Use base class for other checks
        return base.IsValidDistanceFromOthers(position);
    }
    
    /// <summary>
    /// Cleans up destroyed holes from the active list
    /// </summary>
    void CleanupDestroyedHoles()
    {
        activeHoles.RemoveAll(hole => hole == null);
    }
    
    /// <summary>
    /// Called when a hole despawns (callback from MouseHole)
    /// </summary>
    public void OnHoleDespawned()
    {
        CleanupDestroyedHoles();

    }
    
    /// <summary>
    /// Forces a mouse hole spawn (for testing)
    /// </summary>
    public void ForceSpawnHole()
    {
        if (GameManager.Instance != null)
        {
            int currentCheese = GameManager.Instance.GetCurrentCheese();
            SpawnMouseHole(currentCheese);
        }
    }
    
    /// <summary>
    /// Resets spawned thresholds (for new game)
    /// </summary>
    public void ResetSpawnedThresholds()
    {
        spawnedThresholds.Clear();
        activeHoles.Clear();
        currentThresholdIndex = 0;

    }
    
    /// <summary>
    /// Gets the current active holes
    /// </summary>
    public List<MouseHole> GetActiveHoles()
    {
        CleanupDestroyedHoles();
        return new List<MouseHole>(activeHoles);
    }
    
    /// <summary>
    /// Gets the next cheese threshold for hole spawning
    /// </summary>
    public int GetNextThreshold()
    {
        if (GameManager.Instance == null) return -1;
        
        int currentCheese = GameManager.Instance.GetCurrentCheese();
        
        for (int i = 0; i < cheeseThresholds.Length; i++)
        {
            int threshold = cheeseThresholds[i];
            
            // Apply progressive difficulty if enabled
            if (increaseRequirement && i > 0)
            {
                threshold = Mathf.RoundToInt(cheeseThresholds[i] * Mathf.Pow(requirementMultiplier, i));
            }
            
            if (currentCheese < threshold)
            {
                return threshold;
            }
        }
        
        return -1; // No more thresholds
    }
    
    /// <summary>
    /// Shows notification when a hole spawns
    /// </summary>
    void ShowHoleSpawnNotification(int requiredCheese)
    {

        
        // TODO: Show actual UI notification
        // UIManager.Instance?.ShowNotification($"Fare Deliği Belirdi! ({requiredCheese} peynir ile)", 5f);
    }

    /// <summary>
    /// Gets debug information about the spawner state
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Active holes: {activeHoles.Count}, Spawned thresholds: {spawnedThresholds.Count}, Next threshold: {GetNextThreshold()}";
    }
    
    /// <summary>
    /// Creates a simple mouse hole GameObject programmatically
    /// </summary>
    GameObject CreateSimpleMouseHole(Vector3 position)
    {
        GameObject holeObject = new GameObject("MouseHole");
        holeObject.transform.position = position;
        holeObject.tag = "MouseHole";
        
        SpriteRenderer spriteRenderer = holeObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = -1;
        // Sprite will be assigned via prefab - no texture creation here
        
        CircleCollider2D collider = holeObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1f;
        
        MouseHole holeComponent = holeObject.AddComponent<MouseHole>();
        
        return holeObject;
    }
    

}