using UnityEngine;

/// <summary>
/// Base class for all spawners with common spawn area logic
/// </summary>
public abstract class BaseSpawner : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    [SerializeField] protected Transform spawnAreaCenter; // If null, uses transform position
    [SerializeField] protected float spawnRadius = 8f; // Radius for random spawn positions
    [SerializeField] protected float minDistanceFromPlayer = 2f; // Minimum distance from player
    [SerializeField] protected float minDistanceFromOthers = 2f; // Minimum distance from other objects
    [SerializeField] protected LayerMask obstacleLayerMask = 64; // Walls layer to avoid when spawning
    
    [Header("Arena Bounds (Auto-detected)")]
    [SerializeField] protected float wallPadding = 0.1f; // Duvardan uzaklık
    [SerializeField] protected LayerMask wallLayerMask = 64; // Walls layer (layer 6)
    
    protected Transform playerTransform;
    protected Vector2 arenaBounds; // Auto-detected arena bounds
    protected bool boundsDetected = false;
    
    protected virtual void Start()
    {
        // Find player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Auto-detect arena bounds
        DetectArenaBounds();
    }
    
    /// <summary>
    /// Finds a valid spawn position within the spawn area and arena bounds
    /// </summary>
    protected virtual Vector3 FindValidSpawnPosition()
    {
        Vector3 centerPosition = GetSpawnCenter();
        
        // Try multiple positions to find a valid one
        for (int attempts = 0; attempts < 15; attempts++)
        {
            Vector3 candidatePosition = GenerateRandomPosition(centerPosition);
            
            // Check if position is valid
            if (IsValidSpawnPosition(candidatePosition))
            {

                return candidatePosition;
            }
        }
        
        // Fallback to a safe position if no valid position found
        Debug.LogWarning($"{GetType().Name}: Could not find ideal spawn position, using fallback");
        return GetFallbackPosition(centerPosition);
    }
    
    /// <summary>
    /// Generates a random position within spawn radius
    /// Can be overridden for different spawn patterns (e.g., edge spawning for hazards)
    /// </summary>
    protected virtual Vector3 GenerateRandomPosition(Vector3 centerPosition)
    {
        // Better random distribution - use Random.insideUnitCircle directly (not normalized)
        Vector2 randomPoint = Random.insideUnitCircle * spawnRadius;
        
        // Ensure minimum distance from center
        float minRadius = spawnRadius * 0.3f;
        if (randomPoint.magnitude < minRadius)
        {
            randomPoint = randomPoint.normalized * minRadius;
        }
        
        return centerPosition + new Vector3(randomPoint.x, randomPoint.y, 0);
    }
    
    /// <summary>
    /// Auto-detects arena bounds using raycast
    /// </summary>
    protected virtual void DetectArenaBounds()
    {
        Vector3 center = GetSpawnCenter();
        
        // Raycast in 4 directions to find walls
        float maxDistance = 50f; // Maximum raycast distance
        
        // Right wall
        RaycastHit2D rightHit = Physics2D.Raycast(center, Vector2.right, maxDistance, wallLayerMask);
        float rightBound = rightHit.collider != null ? rightHit.point.x - wallPadding : center.x + 10f;
        
        // Left wall
        RaycastHit2D leftHit = Physics2D.Raycast(center, Vector2.left, maxDistance, wallLayerMask);
        float leftBound = leftHit.collider != null ? leftHit.point.x + wallPadding : center.x - 10f;
        
        // Top wall
        RaycastHit2D topHit = Physics2D.Raycast(center, Vector2.up, maxDistance, wallLayerMask);
        float topBound = topHit.collider != null ? topHit.point.y - wallPadding : center.y + 5f;
        
        // Bottom wall
        RaycastHit2D bottomHit = Physics2D.Raycast(center, Vector2.down, maxDistance, wallLayerMask);
        float bottomBound = bottomHit.collider != null ? bottomHit.point.y + wallPadding : center.y - 5f;
        
        // Calculate arena bounds
        float arenaWidth = rightBound - leftBound;
        float arenaHeight = topBound - bottomBound;
        
        arenaBounds = new Vector2(arenaWidth, arenaHeight);
        boundsDetected = true;
        

    }
    
    /// <summary>
    /// Gets the spawn center position
    /// </summary>
    protected Vector3 GetSpawnCenter()
    {
        return spawnAreaCenter != null ? spawnAreaCenter.position : transform.position;
    }
    
    /// <summary>
    /// Gets fallback position when no valid position is found
    /// </summary>
    protected virtual Vector3 GetFallbackPosition(Vector3 centerPosition)
    {
        return centerPosition + Vector3.up * 3f;
    }
    
    /// <summary>
    /// Checks if a spawn position is valid
    /// </summary>
    protected virtual bool IsValidSpawnPosition(Vector3 position)
    {
        // Check arena bounds first
        if (!IsWithinArenaBounds(position))
        {
            return false;
        }
        
        // Check for overlapping obstacles
        if (HasObstacleOverlap(position))
        {
            return false;
        }
        
        // Check distance from player
        if (!IsValidDistanceFromPlayer(position))
        {
            return false;
        }
        
        // Check distance from other objects
        if (!IsValidDistanceFromOthers(position))
        {
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks if position is within arena bounds using raycast
    /// Can be overridden by specific spawners for different behavior
    /// </summary>
    protected virtual bool IsWithinArenaBounds(Vector3 position)
    {
        if (!boundsDetected)
        {
            DetectArenaBounds(); // Try to detect bounds if not done yet
        }
        
        // Use raycast to check if position hits a wall
        Vector3 center = GetSpawnCenter();
        Vector2 direction = (position - center).normalized;
        float distance = Vector2.Distance(position, center);
        
        RaycastHit2D hit = Physics2D.Raycast(center, direction, distance + wallPadding, wallLayerMask);
        
        // If raycast hits a wall before reaching the position, it's outside bounds
        return hit.collider == null;
    }
    
    /// <summary>
    /// Checks for obstacle overlap at position
    /// </summary>
    protected bool HasObstacleOverlap(Vector3 position)
    {
        Collider2D[] overlapping = Physics2D.OverlapCircleAll(position, 1f, obstacleLayerMask);
        return overlapping.Length > 0;
    }
    
    /// <summary>
    /// Checks if position is valid distance from player
    /// </summary>
    protected bool IsValidDistanceFromPlayer(Vector3 position)
    {
        if (playerTransform == null) return true;
        
        float distanceFromPlayer = Vector3.Distance(position, playerTransform.position);
        return distanceFromPlayer >= minDistanceFromPlayer;
    }
    
    /// <summary>
    /// Checks if position is valid distance from other objects
    /// Can be overridden for specific distance requirements
    /// </summary>
    protected virtual bool IsValidDistanceFromOthers(Vector3 position)
    {
        // Check distance from collectibles (cheese)
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (GameObject collectible in collectibles)
        {
            if (Vector3.Distance(position, collectible.transform.position) < minDistanceFromOthers)
            {
                return false;
            }
        }
        
        // Check distance from hazards (cats)
        GameObject[] hazards = GameObject.FindGameObjectsWithTag("Hazard");
        foreach (GameObject hazard in hazards)
        {
            if (Vector3.Distance(position, hazard.transform.position) < minDistanceFromOthers + 1f)
            {
                return false;
            }
        }
        
        // Check distance from mouse holes
        GameObject[] mouseHoles = GameObject.FindGameObjectsWithTag("MouseHole");
        foreach (GameObject hole in mouseHoles)
        {
            if (Vector3.Distance(position, hole.transform.position) < minDistanceFromOthers + 1f)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Updates spawn radius (called by map expansion)
    /// </summary>
    public virtual void UpdateSpawnRadius(float newRadius)
    {
        spawnRadius = newRadius;
        boundsDetected = false; // Force re-detection of bounds
        DetectArenaBounds();

    }
    
    /// <summary>
    /// Forces re-detection of arena bounds
    /// </summary>
    public virtual void ForceDetectBounds()
    {
        boundsDetected = false;
        DetectArenaBounds();
    }
    
    /// <summary>
    /// Gets current spawn radius for external access
    /// </summary>
    public float GetSpawnRadius()
    {
        return spawnRadius;
    }

    /// <summary>
    /// Debug visualization for spawn area
    /// </summary>
    protected virtual void OnDrawGizmosSelected()
    {
        Vector3 centerPosition = GetSpawnCenter();
        
        // Spawn radius'ı göster (yeşil)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(centerPosition, spawnRadius);
        
        // Auto-detected arena sınırlarını göster (kırmızı)
        if (boundsDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(centerPosition, new Vector3(arenaBounds.x, arenaBounds.y, 0));
        }
        
        // Minimum spawn radius'ı göster (sarı)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPosition, spawnRadius * 0.3f);
        
        // Raycast debug lines (mavi)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            float maxDistance = 50f;
            
            // 4 yönde raycast çizgileri
            Gizmos.DrawRay(centerPosition, Vector3.right * maxDistance);
            Gizmos.DrawRay(centerPosition, Vector3.left * maxDistance);
            Gizmos.DrawRay(centerPosition, Vector3.up * maxDistance);
            Gizmos.DrawRay(centerPosition, Vector3.down * maxDistance);
        }
    }
}