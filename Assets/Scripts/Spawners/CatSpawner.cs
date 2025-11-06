using System.Collections;
using UnityEngine;

public class HazardSpawner : BaseSpawner
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject hazardPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float difficultyDecreaseAmount = 0.1f;

    [Header("Warning Indicator")]
    [SerializeField] private bool showWarning = true;
    [SerializeField] private float warningDuration = 1f;
    // warningPrefab removed - auto-created if needed
    [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.5f); // Kırmızı, yarı şeffaf



    [Header("Hazard Settings")]
    [SerializeField] private float hazardSpeed = 5f;
    [SerializeField] private float hazardSpeedIncrement = 0.5f;

    [Header("Population Control")]
    [SerializeField] private int maxCatsInArena = 5; // Maximum cats allowed simultaneously
    
    [Header("Cat-Specific Spawn Settings")]
    [SerializeField] private float topBottomWallAvoidance = 1.5f; // Extra padding to avoid top/bottom walls

    private Coroutine spawnCoroutine;

    // Warning ve Hazard'ın aynı pozisyonda olması için
    private Vector2 nextSpawnPosition;
    private bool nextSpawnFromLeft;
    
    /// <summary>
    /// Override to generate edge-based spawn positions for hazards (left/right sides only)
    /// </summary>
    protected override Vector3 GenerateRandomPosition(Vector3 centerPosition)
    {
        // Generate random position on the edge (left or right side only, avoid top/bottom)
        bool spawnFromLeft = Random.value > 0.5f;
        
        Vector2 randomDirection;
        if (spawnFromLeft)
        {
            // Spawn from left side - reduce vertical range to avoid top/bottom walls
            randomDirection = new Vector2(-1f, Random.Range(-0.3f, 0.3f)).normalized;
        }
        else
        {
            // Spawn from right side - reduce vertical range to avoid top/bottom walls
            randomDirection = new Vector2(1f, Random.Range(-0.3f, 0.3f)).normalized;
        }
        
        float randomDistance = Random.Range(spawnRadius * 0.8f, spawnRadius);
        return centerPosition + new Vector3(randomDirection.x * randomDistance, randomDirection.y * randomDistance, 0);
    }
    
    /// <summary>
    /// Override to prevent cats from spawning near top/bottom walls
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
        
        // Check vertical bounds (top/bottom walls) - use EXTRA padding for cats
        Vector2 verticalDirection = new Vector2(0, Mathf.Sign(position.y - center.y)).normalized;
        float verticalDistance = Mathf.Abs(position.y - center.y);
        RaycastHit2D verticalHit = Physics2D.Raycast(center, verticalDirection, verticalDistance + wallPadding + topBottomWallAvoidance, wallLayerMask);
        
        if (verticalHit.collider != null)
        {
            return false; // Too close to top/bottom wall (with extra avoidance)
        }
        
        return true;
    }

protected override void Start()
    {
        base.Start(); // BaseSpawner'dan player referansını al
        
        // Auto-setup prefab if null
        if (hazardPrefab == null)
        {
            GameObject catPrefab = GameObject.Find("CatPrefab");
            if (catPrefab != null)
            {
                hazardPrefab = catPrefab;

            }
            else
            {
                Debug.LogError("HazardSpawner: No hazard prefab found! Creating a simple one...");
                CreateSimpleHazardPrefab();
            }
        }
    }
    
    void CreateSimpleHazardPrefab()
    {
        hazardPrefab = new GameObject("CatHazardPrefab");
        hazardPrefab.tag = "Hazard";
        
        SpriteRenderer sr = hazardPrefab.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 8;
        // Sprite will be assigned via prefab - no texture creation here
        
        CircleCollider2D collider = hazardPrefab.AddComponent<CircleCollider2D>();
        collider.radius = 0.2f;
        
        Rigidbody2D rb = hazardPrefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.freezeRotation = true;
        
        hazardPrefab.AddComponent<Cat>();
        

    }
    
    // Texture waiting removed - sprites assigned via prefab
    
    // Texture creation removed - using TextureManager only

    /// <summary>
    /// Dynamic spawn area calculation using raycasting
    /// </summary>
// Method removed - using MouseHoleSpawner approach instead

public void StartSpawning()
    {


        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnHazardRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void IncreaseDifficulty()
    {
        // Spawn süresini azalt
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - difficultyDecreaseAmount);

        // Tehlike hızını artır
        hazardSpeed += hazardSpeedIncrement;


    }

    /// <summary>
    /// Updates difficulty based on player upgrades (called when player purchases upgrades)
    /// </summary>
    public void UpdateDifficultyForUpgrades(PlayerPerks playerPerks)
    {
        if (playerPerks == null) return;

        float difficultyMultiplier = 1f;

        // Each upgrade increases overall difficulty
        if (playerPerks.HasSopa) difficultyMultiplier += 0.15f;
        if (playerPerks.CanTeleport) difficultyMultiplier += 0.15f;
        if (playerPerks.HasSpeedBoost) difficultyMultiplier += 0.1f;
        if (playerPerks.HasBuffMagnet) difficultyMultiplier += 0.1f;
        if (playerPerks.HasMapExpansion) difficultyMultiplier += 0.2f;

        // Apply difficulty multiplier to spawn rate
        float baseInterval = 2f; // Original spawn interval
        spawnInterval = Mathf.Max(minSpawnInterval, baseInterval / difficultyMultiplier);

        // Apply to hazard speed
        float baseSpeed = 5f; // Original hazard speed
        hazardSpeed = baseSpeed * difficultyMultiplier;


    }

    /// <summary>
    /// Legacy method - cats now use chance-based abilities determined at spawn
    /// </summary>
    public void UpdateExistingHazardsBehavior(PlayerPerks playerPerks)
    {
        // Deprecated - cat abilities are now determined by chance at spawn time

    }

    /// <summary>
    /// Sets spawn interval directly (used by DifficultyManager)
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(minSpawnInterval, interval);

    }

    /// <summary>
    /// Sets hazard speed directly (used by DifficultyManager)
    /// </summary>
    public void SetHazardSpeed(float speed)
    {
        hazardSpeed = speed;

    }

    /// <summary>
    /// Gets current number of active cats in scene
    /// </summary>
    public int GetActiveHazardCount()
    {
        return FindObjectsByType<Cat>(FindObjectsSortMode.None).Length;
    }

    /// <summary>
    /// Sets maximum cats allowed in arena (used by DifficultyManager)
    /// </summary>
    public void SetMaxCatsInArena(int maxCats)
    {
        maxCatsInArena = maxCats;

    }

    /// <summary>
    /// Gets current spawn parameters for debugging
    /// </summary>
    public string GetSpawnInfo()
    {
        return $"Interval: {spawnInterval:F2}s, Speed: {hazardSpeed:F1}, Active: {GetActiveHazardCount()}/{maxCatsInArena}";
    }
    
    /// <summary>
    /// Increases spawn rate and difficulty (used by upgrade system)
    /// </summary>
    public void IncreaseSpawnRate(float multiplier)
    {
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval / multiplier);
        maxCatsInArena = Mathf.RoundToInt(maxCatsInArena * multiplier);
        

    }

    IEnumerator SpawnHazardRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // ÖNCELİKLE pozisyonu hesapla (Warning ve Hazard aynı yerde olacak)
            CalculateNextSpawnPosition();

            // Warning göster (eğer aktifse)
            if (showWarning)
            {
                ShowWarning();
                yield return new WaitForSeconds(warningDuration);
            }

            SpawnHazard();
        }
    }

    /// <summary>
    /// Bir sonraki spawn pozisyonunu hesapla (Warning ve Hazard için aynı)
    /// </summary>
void CalculateNextSpawnPosition()
    {
        Vector3 centerPosition = GetSpawnCenter();
        
        // Try multiple positions to avoid overlap
        for (int attempts = 0; attempts < 15; attempts++)
        {
            // Generate random position on the edge (left or right side)
            nextSpawnFromLeft = Random.value > 0.5f;
            
            Vector2 randomDirection;
            if (nextSpawnFromLeft)
            {
                // Spawn from left side
                randomDirection = new Vector2(-1f, Random.Range(-0.5f, 0.5f)).normalized;
            }
            else
            {
                // Spawn from right side
                randomDirection = new Vector2(1f, Random.Range(-0.5f, 0.5f)).normalized;
            }
            
            float randomDistance = Random.Range(spawnRadius * 0.8f, spawnRadius);
            Vector2 candidatePosition = (Vector2)centerPosition + randomDirection * randomDistance;
            
            // Check if position is clear from other objects
            if (IsValidSpawnPosition(candidatePosition))
            {
                nextSpawnPosition = candidatePosition;

                return;
            }
        }
        
        // Fallback to base spawner method
        nextSpawnPosition = FindValidSpawnPosition();
        nextSpawnFromLeft = nextSpawnPosition.x < GetSpawnCenter().x;
        Debug.LogWarning("HazardSpawner: Using base spawner fallback position");
    }




    /// <summary>
    /// Hazard spawn olmadan önce warning göster
    /// </summary>
    void ShowWarning()
    {
        // Önceden hesaplanmış pozisyonu kullan
        Vector2 warningPosition = nextSpawnPosition;

        // Warning objesi oluştur
        GameObject warning = null;

        // Basit bir warning sprite oluştur
        warning = new GameObject("HazardWarning");
        warning.transform.position = warningPosition;

        SpriteRenderer sr = warning.AddComponent<SpriteRenderer>();
        sr.sprite = CreateWarningSprite();
        sr.color = warningColor;
        sr.sortingOrder = 10; // En üstte görünsün

        // Yanıp sönme script'i ekle
        warning.AddComponent<HazardWarningIndicator>();

        // Warning'i otomatik yok et
        Destroy(warning, warningDuration);
    }

    /// <summary>
    /// Creates a simple warning sprite
    /// </summary>
    Sprite CreateWarningSprite()
    {
        // Create a simple 32x32 red texture for warning
        Texture2D warningTexture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        // Fill with warning color
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.red;
        }
        
        warningTexture.SetPixels(pixels);
        warningTexture.Apply();
        
        // Create sprite from texture
        Sprite warningSprite = Sprite.Create(warningTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        
        return warningSprite;
    }

    public void SpawnHazard()
    {
        // Auto-create prefab if null
        if (hazardPrefab == null)
        {
            CreateSimpleHazardPrefab();
            
            if (hazardPrefab == null)
            {
                Debug.LogError("HazardSpawner: Failed to create hazard prefab!");
                return;
            }
        }

        // Check if we've reached the maximum cat limit
        if (GetActiveHazardCount() >= maxCatsInArena)
        {

            return;
        }

        // Önceden hesaplanmış pozisyonu kullan (Warning ile aynı yerde)
        Vector2 spawnPosition = nextSpawnPosition;
        bool spawnFromLeft = nextSpawnFromLeft;

        // Tehlike objesini spawn et
        GameObject hazard = Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);
        
        // TextureManager'dan texture uygula
        if (TextureManager.Instance != null)
        {
            TextureManager.Instance.ApplyTextureToNewObject(hazard, "hazard");
        }

        // Hız ayarla (soldan spawn oluyorsa sağa, sağdan spawn oluyorsa sola hareket et)
        Cat catScript = hazard.GetComponent<Cat>();
        if (catScript != null)
        {
            // AÇIK YÖN KONTROLÜ: Sol taraftan spawn = pozitif hız (sağa), Sağ taraftan spawn = negatif hız (sola)
            float speed = spawnFromLeft ? Mathf.Abs(hazardSpeed) : -Mathf.Abs(hazardSpeed);
            catScript.SetSpeed(speed);

            // Cat abilities are now determined by ApplyChanceBasedAbilities() in Start()
            // No need for additional behavior updates here
        }
        else
        {
            Debug.LogError("HazardSpawner: Cat script not found on spawned object!");
        }

        string side = spawnFromLeft ? "SOL" : "SAĞ";
        string direction = spawnFromLeft ? "SAĞA" : "SOLA";
        float actualSpeed = spawnFromLeft ? Mathf.Abs(hazardSpeed) : -Mathf.Abs(hazardSpeed);

    }
    
    // Texture application removed - handled via prefab

    // Debug için spawn alanını görselleştir - BaseSpawner'dan override
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // Base visualization
        
        // Additional visualization for edge spawning
        Vector3 centerPosition = GetSpawnCenter();
        Gizmos.color = Color.blue;
        Vector3 leftSpawn = centerPosition + Vector3.left * spawnRadius;
        Vector3 rightSpawn = centerPosition + Vector3.right * spawnRadius;
        
        Gizmos.DrawWireCube(leftSpawn, new Vector3(1f, spawnRadius, 0f));
        Gizmos.DrawWireCube(rightSpawn, new Vector3(1f, spawnRadius, 0f));
    }
}
