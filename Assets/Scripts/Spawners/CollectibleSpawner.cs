using UnityEngine;

public class CollectibleSpawner : BaseSpawner
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab; // Auto-created if null
    
    [Header("Cheese-Specific Spawn Settings")]
    [SerializeField] private float topBottomWallAllowance = 0.5f; // Allow cheese closer to top/bottom walls

protected override void Start()
    {
        base.Start(); // BaseSpawner'dan player referansını al
        
        // TextureManager'ı bekle
        if (TextureManager.Instance == null)
        {

        }
    }
    
public Vector3 GetSafeSpawnPosition()
    {
        return FindValidSpawnPosition(); // BaseSpawner'dan kullan
    }

    
    /// <summary>
    /// Override to allow cheese spawning closer to top/bottom walls
    /// </summary>
    protected override bool IsWithinArenaBounds(Vector3 position)
    {
        if (!boundsDetected)
        {
            DetectArenaBounds(); // Try to detect bounds if not done yet
        }
        
        Vector3 center = GetSpawnCenter();
        
        // Check horizontal bounds (left/right walls) - use normal padding
        Vector2 horizontalDirection = new Vector2(Mathf.Sign(position.x - center.x), 0).normalized;
        float horizontalDistance = Mathf.Abs(position.x - center.x);
        RaycastHit2D horizontalHit = Physics2D.Raycast(center, horizontalDirection, horizontalDistance + wallPadding, wallLayerMask);
        
        if (horizontalHit.collider != null)
        {
            return false; // Too close to left/right wall
        }
        
        // Check vertical bounds (top/bottom walls) - use reduced padding for cheese
        Vector2 verticalDirection = new Vector2(0, Mathf.Sign(position.y - center.y)).normalized;
        float verticalDistance = Mathf.Abs(position.y - center.y);
        RaycastHit2D verticalHit = Physics2D.Raycast(center, verticalDirection, verticalDistance + topBottomWallAllowance, wallLayerMask);
        
        if (verticalHit.collider != null)
        {
            return false; // Too close to top/bottom wall (but with reduced padding)
        }
        
        return true;
    }

    public void SpawnCollectible()
    {
        // Auto-create prefab if null
        if (collectiblePrefab == null)
        {
            CreateSimpleCollectiblePrefab();
        }

        // Dinamik hesaplanan alan içinde rastgele pozisyon
        Vector2 randomPosition = GetSafeSpawnPosition();

        // Puan objesini spawn et
        GameObject spawned = Instantiate(collectiblePrefab, randomPosition, Quaternion.identity);
        
        // IMPORTANT: Activate the spawned object
        spawned.SetActive(true);
        
        // TextureManager'dan texture uygula
        if (TextureManager.Instance != null)
        {
            TextureManager.Instance.ApplyTextureToNewObject(spawned, "collectible");
        }


    }
    
    void ApplyCheeseTextureToSpawned(GameObject spawned)
    {
        if (TextureManager.Instance != null && TextureManager.Instance.cheeseSprite != null)
        {
            SpriteRenderer sr = spawned.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = TextureManager.Instance.cheeseSprite;

            }
        }
    }
    
    void CreateSimpleCollectiblePrefab()
    {
        collectiblePrefab = new GameObject("CheeseCollectiblePrefab");
        collectiblePrefab.tag = "Collectible";
        
        SpriteRenderer sr = collectiblePrefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        // Sprite will be assigned via prefab - no texture creation here
        
        CircleCollider2D collider = collectiblePrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.1f;
        
        collectiblePrefab.AddComponent<Collectible>();
        collectiblePrefab.SetActive(false);
        

    }
    
    // Texture waiting removed - sprites assigned via prefab


}
