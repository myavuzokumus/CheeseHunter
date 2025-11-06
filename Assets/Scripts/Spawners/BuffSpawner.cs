using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages spawning of temporary buffs in the arena
/// </summary>
public class BuffSpawner : BaseSpawner
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] buffPrefabs;
    [SerializeField] private float minSpawnInterval = 15f;
    [SerializeField] private float maxSpawnInterval = 30f;
    [SerializeField] private int maxBuffsInArena = 2;
    
    private Coroutine spawnCoroutine;
    private List<BuffCollectible> activeBuffs = new List<BuffCollectible>();
    private float spawnRateMultiplier = 1f;
    
    protected override void Start()
    {
        base.Start(); // BaseSpawner'dan player referansını al
        
        // Set spawn radius to 12 for buffs
        spawnRadius = 12f;
        minDistanceFromPlayer = 3f; // Buffs need distance from player
        
        // Auto-setup buff prefabs if null
        if (buffPrefabs == null || buffPrefabs.Length == 0)
        {
            GameObject buffPrefab = GameObject.Find("BuffPrefab");
            if (buffPrefab != null)
            {
                buffPrefabs = new GameObject[] { buffPrefab };

            }
            else
            {
                CreateDefaultBuffPrefab();
            }
        }
        
        // Validate buff prefabs
        bool hasValidPrefabs = false;
        foreach (GameObject prefab in buffPrefabs)
        {
            if (prefab != null)
            {
                hasValidPrefabs = true;
                break;
            }
        }
        
        if (!hasValidPrefabs)
        {
            Debug.LogError("BuffSpawner: No valid buff prefabs found! Disabling spawner.");
            enabled = false;
            return;
        }
        

    }
    
    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnBuffRoutine());

    }
    
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        

    }
    
    IEnumerator SpawnBuffRoutine()
    {
        while (true)
        {
            float baseInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            float actualInterval = baseInterval / spawnRateMultiplier;
            
            yield return new WaitForSeconds(actualInterval);
            
            CleanupActiveBuffs();
            if (activeBuffs.Count < maxBuffsInArena)
            {
                SpawnRandomBuff();
            }
        }
    }
    
    void SpawnRandomBuff()
    {
        if (buffPrefabs.Length == 0) return;
        
        GameObject buffPrefab = buffPrefabs[Random.Range(0, buffPrefabs.Length)];
        
        if (buffPrefab == null)
        {
            Debug.LogWarning("BuffSpawner: Selected buff prefab is null!");
            return;
        }
        
        Vector3 spawnPosition = FindValidSpawnPosition();
        
        // Show buff activation effect 1 second before spawning
        StartCoroutine(SpawnBuffWithPreEffect(buffPrefab, spawnPosition));
    }
    
    /// <summary>
    /// Spawns buff with pre-spawn effect
    /// </summary>
    IEnumerator SpawnBuffWithPreEffect(GameObject buffPrefab, Vector3 spawnPosition)
    {
        // Show effect 1 second before spawn
        VFXManager.Instance?.PlayBuffActivationEffect(spawnPosition);
        
        yield return new WaitForSeconds(1f);
        
        // Now spawn the actual buff
        GameObject buffObject = Instantiate(buffPrefab, spawnPosition, Quaternion.identity);
        
        // TextureManager'dan texture uygula
        if (TextureManager.Instance != null)
        {
            TextureManager.Instance.ApplyTextureToNewObject(buffObject, "buff");
        }
        
        BuffCollectible buffComponent = buffObject.GetComponent<BuffCollectible>();
        
        if (buffComponent != null)
        {
            // Check if player has BuffMagnet upgrade and enable magnet effect
            PlayerPerks playerPerks = FindFirstObjectByType<PlayerPerks>();
            if (playerPerks != null && playerPerks.HasBuffMagnet)
            {
                buffComponent.EnableMagnetEffect(playerPerks.BuffMagnetRange);
            }
            
            activeBuffs.Add(buffComponent);

        }
    }
    
    /// <summary>
    /// Override to add buff-specific distance checks
    /// </summary>
    protected override bool IsValidDistanceFromOthers(Vector3 position)
    {
        // Check distance from existing buffs first
        foreach (BuffCollectible buff in activeBuffs)
        {
            if (buff != null)
            {
                float distanceFromBuff = Vector3.Distance(position, buff.transform.position);
                if (distanceFromBuff < minDistanceFromOthers) return false;
            }
        }
        
        // Use base class for other checks
        return base.IsValidDistanceFromOthers(position);
    }
    
    // Texture application removed - handled via prefab
    
    void CleanupActiveBuffs()
    {
        activeBuffs.RemoveAll(buff => buff == null);
    }
    
    public void ResetSpawner()
    {
        activeBuffs.Clear();
        spawnRateMultiplier = 1f;

    }
    
    public int GetActiveBuffCount()
    {
        CleanupActiveBuffs();
        return activeBuffs.Count;
    }
    
    public void ForceSpawnBuff()
    {
        if (activeBuffs.Count < maxBuffsInArena)
        {
            SpawnRandomBuff();
        }
    }
    
    /// <summary>
    /// Enables magnet effect for newly spawned buffs (called when player purchases buff magnet upgrade)
    /// </summary>
    public void EnableMagnetEffectForNewBuffs()
    {
        // Enable magnet effect for all existing buffs
        CleanupActiveBuffs();
        foreach (BuffCollectible buff in activeBuffs)
        {
            if (buff != null)
            {
                PlayerPerks playerPerks = FindFirstObjectByType<PlayerPerks>();
                if (playerPerks != null)
                {
                    buff.EnableMagnetEffect(playerPerks.BuffMagnetRange);
                }
            }
        }
        
        // This will affect future spawned buffs too

    }
    
    void CreateDefaultBuffPrefab()
    {
        GameObject buffPrefab = new GameObject("DefaultBuffPrefab");
        buffPrefab.tag = "Buff";
        buffPrefab.transform.localScale = Vector3.one * 0.4f;
        
        SpriteRenderer sr = buffPrefab.AddComponent<SpriteRenderer>();
        // Sprite will be assigned by TextureManager
        sr.sortingOrder = 6;
        
        CircleCollider2D collider = buffPrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.4f;
        
        buffPrefab.AddComponent<BuffCollectible>();
        
        buffPrefabs = new GameObject[] { buffPrefab };
        

    }
    
    /// <summary>
    /// Reduces spawn rate (negative effect of BuffMagnet upgrade)
    /// </summary>
    public void ReduceSpawnRate(float multiplier)
    {
        spawnRateMultiplier *= multiplier;
        maxBuffsInArena = Mathf.Max(1, Mathf.RoundToInt(maxBuffsInArena * multiplier));
        

    }
    
    /// <summary>
    /// Sets spawn rate multiplier directly
    /// </summary>
    public void SetSpawnRateMultiplier(float multiplier)
    {
        spawnRateMultiplier = multiplier;

    }
    
    /// <summary>
    /// Gets current spawn rate info for debugging
    /// </summary>
    public string GetSpawnInfo()
    {
        return $"Rate Multiplier: {spawnRateMultiplier:F2}, Active: {GetActiveBuffCount()}/{maxBuffsInArena}";
    }
    

}