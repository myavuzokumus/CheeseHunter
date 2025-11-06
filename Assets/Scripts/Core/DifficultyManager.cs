using UnityEngine;

/// <summary>
/// Manages dynamic difficulty scaling based on score and player upgrades
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [SerializeField] private int[] difficultyThresholds = { 15, 30, 45, 60, 75, 90 };
    [SerializeField] private float baseSpawnInterval = 2f;
    [SerializeField] private float minSpawnInterval = 0.3f;
    [SerializeField] private float baseHazardSpeed = 5f;
    [SerializeField] private int baseCatCount = 3;
    [SerializeField] private int maxCatCount = 8;
    
    private int currentDifficultyLevel = 0;
    private HazardSpawner hazardSpawner;
    
    void Start()
    {
        hazardSpawner = FindFirstObjectByType<HazardSpawner>();
        ResetDifficulty();
    }
    
    void Update()
    {
        CheckDifficultyIncrease();
    }
    
    void CheckDifficultyIncrease()
    {
        if (GameManager.Instance == null) return;
        
        int currentCheese = GameManager.Instance.GetCurrentCheese();
        
        // Check if we should increase difficulty
        for (int i = currentDifficultyLevel; i < difficultyThresholds.Length; i++)
        {
            if (currentCheese >= difficultyThresholds[i])
            {
                IncreaseDifficulty();
                currentDifficultyLevel = i + 1;
            }
        }
    }
    
    void IncreaseDifficulty()
    {
        if (hazardSpawner == null) return;
        
        // Calculate new difficulty values
        float difficultyMultiplier = 1f + (currentDifficultyLevel * 0.2f);
        
        // Decrease spawn interval
        float newSpawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval / difficultyMultiplier);
        hazardSpawner.SetSpawnInterval(newSpawnInterval);
        
        // Increase hazard speed
        float newHazardSpeed = baseHazardSpeed * difficultyMultiplier;
        hazardSpawner.SetHazardSpeed(newHazardSpeed);
        
        // Increase max cats
        int newMaxCats = Mathf.Min(maxCatCount, baseCatCount + currentDifficultyLevel);
        hazardSpawner.SetMaxCatsInArena(newMaxCats);
        
        Debug.Log($"DifficultyManager: Difficulty increased to level {currentDifficultyLevel}! " +
                  $"Interval: {newSpawnInterval:F2}s, Speed: {newHazardSpeed:F1}, MaxCats: {newMaxCats}");
    }
    
    public void UpdateDifficultyForUpgrades(PlayerPerks playerPerks)
    {
        if (playerPerks == null || hazardSpawner == null) return;
        
        // Additional difficulty scaling based on upgrades
        float upgradeMultiplier = 1f;
        
        if (playerPerks.HasSpeedBoost) upgradeMultiplier += 0.1f;
        if (playerPerks.HasSopa) upgradeMultiplier += 0.15f;
        if (playerPerks.CanTeleport) upgradeMultiplier += 0.15f;
        if (playerPerks.HasBuffMagnet) upgradeMultiplier += 0.1f;
        if (playerPerks.HasMapExpansion) upgradeMultiplier += 0.2f;
        
        // Apply upgrade-based difficulty to spawner
        hazardSpawner.UpdateDifficultyForUpgrades(playerPerks);
        
        // Force a difficulty level increase to affect new cat spawns
        if (upgradeMultiplier > 1.2f) // If player has multiple upgrades
        {
            currentDifficultyLevel = Mathf.Min(currentDifficultyLevel + 1, difficultyThresholds.Length);
            Debug.Log($"DifficultyManager: Forced difficulty increase due to upgrades - Level: {currentDifficultyLevel}");
        }
        
        Debug.Log($"DifficultyManager: Upgrade difficulty applied - Multiplier: {upgradeMultiplier:F2}, Level: {currentDifficultyLevel}");
    }
    
    public void ResetDifficulty()
    {
        currentDifficultyLevel = 0;
        
        if (hazardSpawner != null)
        {
            hazardSpawner.SetSpawnInterval(baseSpawnInterval);
            hazardSpawner.SetHazardSpeed(baseHazardSpeed);
            hazardSpawner.SetMaxCatsInArena(baseCatCount);
        }
        
        Debug.Log("DifficultyManager: Difficulty reset to base level");
    }
    
    public void ForceDifficultyIncrease()
    {
        IncreaseDifficulty();
        currentDifficultyLevel++;
    }
    
    public DifficultyInfo GetCurrentDifficultyInfo()
    {
        return new DifficultyInfo
        {
            level = currentDifficultyLevel,
            spawnInterval = hazardSpawner?.GetSpawnInfo() ?? "N/A",
            nextThreshold = currentDifficultyLevel < difficultyThresholds.Length ? 
                           difficultyThresholds[currentDifficultyLevel] : -1
        };
    }
}

[System.Serializable]
public class DifficultyInfo
{
    public int level;
    public string spawnInterval;
    public int nextThreshold;
}