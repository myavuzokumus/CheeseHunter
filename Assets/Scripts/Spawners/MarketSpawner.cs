using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawns market triggers at specific cheese thresholds
/// </summary>
public class MarketSpawner : BaseSpawner
{
    [Header("Market Settings")]
    [SerializeField] private GameObject marketPrefab;
    [SerializeField] private int[] cheeseThresholds = { 5, 15, 30, 50, 80 }; // İlk market 5 peynirde
    [SerializeField] private float marketLifetime = 30f;
    
    private HashSet<int> spawnedThresholds = new HashSet<int>();
    private GameObject currentMarket;
    
protected override void Start()
    {
        base.Start(); // BaseSpawner'dan player referansını al
        
        // Create market prefab if it doesn't exist
        if (marketPrefab == null)
        {
            CreateMarketPrefab();
        }
        
        // Set spawn radius to 12 for market coverage
        spawnRadius = 12f;
        

    }
    
void Update()
    {
        // Only check if GameManager exists and game is playing
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying())
        {
            CheckForMarketSpawn();
        }
    }
    
// Removed debug spam
    
    void CreateMarketPrefab()
    {
        marketPrefab = new GameObject("MarketPrefab");
        marketPrefab.tag = "Market";
        
        SpriteRenderer sr = marketPrefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        sr.color = Color.green; // Temporary color to make it visible
        
        // Create a simple sprite for visibility
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.green;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite marketSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        sr.sprite = marketSprite;
        
        // Collider for interaction - MUST BE TRIGGER
        CircleCollider2D collider = marketPrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1f; // Bigger hitbox for easier testing
        
        // Market trigger component
        marketPrefab.AddComponent<MarketTrigger>();
        
        // Set scale for visibility
        marketPrefab.transform.localScale = Vector3.one * 0.5f;
        
        // Deactivate prefab so it doesn't interfere with gameplay
        marketPrefab.SetActive(false);
        

    }
    
    // Texture creation removed - using TextureManager only
    Sprite CreateMarketSprite()
    {
        return null; // No fallback sprite - must use TextureManager
    }
    
public void CheckForMarketSpawn()
    {
        if (currentMarket != null) return; // Market zaten var, spam'i önle
        
        if (GameManager.Instance == null) return;
        
        int currentCheese = GameManager.Instance.GetCurrentCheese();
        
        // Check each threshold only once
        foreach (int threshold in cheeseThresholds)
        {
            if (currentCheese >= threshold && !spawnedThresholds.Contains(threshold))
            {

                SpawnMarket();
                spawnedThresholds.Add(threshold);
                break; // Sadece bir market spawn et
            }
        }
    }
    
void SpawnMarket()
    {
        if (currentMarket != null) return; // Only one market at a time
        
        if (marketPrefab == null)
        {
            Debug.LogError("MarketSpawner: Market prefab is null! Cannot spawn market.");
            return;
        }
        
        Vector3 spawnPosition = FindValidSpawnPosition();
        currentMarket = Instantiate(marketPrefab, spawnPosition, Quaternion.identity);
        
        // IMPORTANT: Activate the market (prefab is deactivated by default)
        if (currentMarket != null)
        {
            currentMarket.SetActive(true);
            
            // TextureManager'dan texture uygula
            if (TextureManager.Instance != null)
            {
                TextureManager.Instance.ApplyTextureToNewObject(currentMarket, "market");
            }
            
            // Sorting order ayarla
            SpriteRenderer sr = currentMarket.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 10; // En üstte görünsün
            }
            
            // Auto-destroy after lifetime
            Destroy(currentMarket, marketLifetime);
            

        }
        else
        {
            Debug.LogError("MarketSpawner: Failed to instantiate market!");
        }
    }




    
    /// <summary>
    /// Force spawn a market for testing (called by GameTester)
    /// </summary>
    public void ForceSpawnMarket()
    {
        // Remove current market if exists
        if (currentMarket != null)
        {
            Destroy(currentMarket);
            currentMarket = null;
        }
        
        SpawnMarket();

    }
    

    
    public void ResetSpawnedThresholds()
    {
        spawnedThresholds.Clear();
        if (currentMarket != null)
        {
            Destroy(currentMarket);
            currentMarket = null;
        }
    }
    

}