using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    private int currentCheese = 0;
    private int highScore = 0;
    private GameState currentGameState = GameState.Playing;
    private float gameTime = 0f;

    [Header("UI References")]
    [SerializeField] private GameUI gameUI;

    [Header("Spawner References")]
    [SerializeField] private CollectibleSpawner collectibleSpawner;
    [SerializeField] private HazardSpawner hazardSpawner;
    [SerializeField] private MarketSpawner marketSpawner;
    [SerializeField] private DifficultyManager difficultyManager;
    [SerializeField] private BuffSpawner buffSpawner;
    [SerializeField] private MouseHoleSpawner mouseHoleSpawner;

    [Header("Difficulty Settings")]
    [SerializeField] private int scoreThresholdForDifficulty = 5;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Auto-setup references if they're null
        AutoSetupReferences();

        // Yüksek skoru yükle
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Oyunu başlat
        StartGame();
    }

    void AutoSetupReferences()
    {
        // Find GameUI component
        if (gameUI == null)
        {
            gameUI = FindFirstObjectByType<GameUI>();
            if (gameUI == null)
            {
                Debug.LogWarning("GameManager: GameUI not found in scene!");
            }
        }

        // Find spawner references with retry logic
        if (collectibleSpawner == null)
        {
            collectibleSpawner = FindFirstObjectByType<CollectibleSpawner>();
            if (collectibleSpawner == null)
            {
                Debug.LogWarning("GameManager: CollectibleSpawner not found, will retry later");
            }
        }

        if (hazardSpawner == null)
        {
            hazardSpawner = FindFirstObjectByType<HazardSpawner>();
            if (hazardSpawner == null)
            {
                Debug.LogWarning("GameManager: HazardSpawner not found, will retry later");
            }
        }

        if (marketSpawner == null)
        {
            marketSpawner = FindFirstObjectByType<MarketSpawner>();
        }

        if (buffSpawner == null)
        {
            buffSpawner = FindFirstObjectByType<BuffSpawner>();
        }

        if (mouseHoleSpawner == null)
        {
            mouseHoleSpawner = FindFirstObjectByType<MouseHoleSpawner>();
        }

        if (difficultyManager == null)
        {
            difficultyManager = GetComponent<DifficultyManager>();
        }
    }

    void Update()
    {
        if (currentGameState == GameState.Playing)
        {
            gameTime += Time.deltaTime;
        }
    }

    void StartGame()
    {
        currentCheese = 0;
        currentGameState = GameState.Playing;
        gameTime = 0f;
        Time.timeScale = 1f;

        UpdateCheeseUI();

        // Retry finding spawners if they're null
        if (collectibleSpawner == null)
            collectibleSpawner = FindFirstObjectByType<CollectibleSpawner>();
        if (hazardSpawner == null)
            hazardSpawner = FindFirstObjectByType<HazardSpawner>();
        if (buffSpawner == null)
            buffSpawner = FindFirstObjectByType<BuffSpawner>();
        if (marketSpawner == null)
            marketSpawner = FindFirstObjectByType<MarketSpawner>();
        if (mouseHoleSpawner == null)
            mouseHoleSpawner = FindFirstObjectByType<MouseHoleSpawner>();

        // Reset spawners for new game
        if (marketSpawner != null)
            marketSpawner.ResetSpawnedThresholds();

        if (difficultyManager != null)
            difficultyManager.ResetDifficulty();

        if (buffSpawner != null)
            buffSpawner.ResetSpawner();

        if (mouseHoleSpawner != null)
            mouseHoleSpawner.ResetSpawnedThresholds();

        // İlk puan objesini spawn et
        if (collectibleSpawner != null)
        {
            collectibleSpawner.SpawnCollectible();
        }
        else
        {
            Debug.LogWarning("GameManager: CollectibleSpawner still null after retry!");
        }

        // Tehlike spawn'ını başlat (lazy-load if needed)
        if (hazardSpawner == null)
            hazardSpawner = FindFirstObjectByType<HazardSpawner>();
        if (hazardSpawner != null)
            hazardSpawner.StartSpawning();
        else
            Debug.LogWarning("GameManager: HazardSpawner not found, cannot start spawning");

        // Buff spawn'ını başlat (lazy-load if needed)
        if (buffSpawner == null)
            buffSpawner = FindFirstObjectByType<BuffSpawner>();
        if (buffSpawner != null)
            buffSpawner.StartSpawning();
        else
            Debug.LogWarning("GameManager: BuffSpawner not found, cannot start spawning");
    }

    public void AddCheese(int amount)
    {
        if (currentGameState != GameState.Playing) return;

        int previousCheese = currentCheese;
        currentCheese += amount;
        UpdateCheeseUI();

        // Comprehensive system updates on cheese collection
        OnCheeseCollected(previousCheese, currentCheese);

        // Yeni puan objesi spawn et (lazy-load spawner if needed)
        if (collectibleSpawner == null)
        {
            collectibleSpawner = FindFirstObjectByType<CollectibleSpawner>();
        }
        
        if (collectibleSpawner != null)
        {
            collectibleSpawner.SpawnCollectible();
        }
        else
        {
            Debug.LogWarning("GameManager: CollectibleSpawner still not found in scene");
        }
    }

    public bool SpendCheese(int amount)
    {
        if (currentCheese >= amount)
        {
            currentCheese -= amount;
            UpdateCheeseUI();
            return true;
        }
        return false;
    }

    public bool CanAfford(int amount)
    {
        return currentCheese >= amount;
    }

    public int GetCurrentCheese()
    {
        return currentCheese;
    }

    void UpdateCheeseUI()
    {
        if (gameUI != null)
            gameUI.UpdateScore(currentCheese);
    }

    void IncreaseDifficulty()
    {
        // Her 5 peynirde bir zorluk artır
        if (currentCheese % scoreThresholdForDifficulty == 0 && currentCheese > 0)
        {
            if (hazardSpawner != null)
                hazardSpawner.IncreaseDifficulty();
        }
    }

    public void GameOver()
    {
        if (currentGameState == GameState.GameOver || currentGameState == GameState.Victory) return;

        // Play death effect at player position before changing game state
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayDeathEffect(player.transform.position);
        }

        currentGameState = GameState.GameOver;
        Time.timeScale = 0f;

        // Oyun bitti UI'ını göster
        ShowGameOverPanel();

        // Tehlike spawn'ını durdur
        if (hazardSpawner != null)
            hazardSpawner.StopSpawning();

        // Buff spawn'ını durdur
        if (buffSpawner != null)
            buffSpawner.StopSpawning();
    }

    public void WinGame()
    {
        if (currentGameState == GameState.GameOver || currentGameState == GameState.Victory) return;

        currentGameState = GameState.Victory;
        Time.timeScale = 0f;

        // Yüksek skor kontrolü
        if (currentCheese > highScore)
        {
            highScore = currentCheese;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Zafer UI'ını göster
        ShowVictoryPanel();

        // Tehlike spawn'ını durdur
        if (hazardSpawner != null)
            hazardSpawner.StopSpawning();

        // Buff spawn'ını durdur
        if (buffSpawner != null)
            buffSpawner.StopSpawning();
    }

    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            ChangeGameState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused || currentGameState == GameState.Market)
        {
            ChangeGameState(GameState.Playing);
            Time.timeScale = 1f;
        }
    }

    public void SetMarketState()
    {
        ChangeGameState(GameState.Market);
        Time.timeScale = 0f;
    }

    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }
    
    public bool IsPlaying()
    {
        return currentGameState == GameState.Playing;
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    void ShowGameOverPanel()
    {
        if (gameUI != null)
        {
            gameUI.ShowGameOver(currentCheese, highScore);
        }
    }

    void ShowVictoryPanel()
    {
        if (gameUI != null)
        {
            gameUI.ShowVictory(currentCheese, highScore, gameTime);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public int GetCurrentScore()
    {
        return currentCheese;
    }

    public PlayerPerks GetPlayerPerks()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.GetComponent<PlayerPerks>();
        }
        return null;
    }

    public MarketSpawner GetMarketSpawner()
    {
        return marketSpawner;
    }

    public void ForceSpawnMarket()
    {
        if (marketSpawner != null)
        {
            marketSpawner.ForceSpawnMarket();
        }
    }

    public void UpdateCatAIDifficulty()
    {
        PlayerPerks playerPerks = GetPlayerPerks();
        if (playerPerks != null)
        {
            if (difficultyManager != null)
            {
                difficultyManager.UpdateDifficultyForUpgrades(playerPerks);
            }
            
            if (hazardSpawner != null)
            {
                hazardSpawner.UpdateDifficultyForUpgrades(playerPerks);
            }
        }
    }

    public DifficultyManager GetDifficultyManager()
    {
        return difficultyManager;
    }

    public void ForceDifficultyIncrease()
    {
        if (difficultyManager != null)
        {
            difficultyManager.ForceDifficultyIncrease();
        }
    }

    public BuffSpawner GetBuffSpawner()
    {
        return buffSpawner;
    }

    public void ForceSpawnBuff()
    {
        if (buffSpawner != null)
        {
            buffSpawner.ForceSpawnBuff();
        }
    }

    public MouseHoleSpawner GetMouseHoleSpawner()
    {
        return mouseHoleSpawner;
    }

    public void ForceSpawnMouseHole()
    {
        if (mouseHoleSpawner != null)
        {
            mouseHoleSpawner.ForceSpawnHole();
        }
    }

    private void OnCheeseCollected(int previousAmount, int newAmount)
    {
        // Trigger event system notification
        GameEventSystem.TriggerCheeseChanged(previousAmount, newAmount);

        // Legacy difficulty increase (kept for compatibility)
        IncreaseDifficulty();

        // Trigger market spawning checks
        if (marketSpawner != null)
        {
            marketSpawner.CheckForMarketSpawn();
        }

        // Trigger mouse hole spawning checks
        if (mouseHoleSpawner != null)
        {
            // Mouse hole spawner has its own Update loop
        }

        // Update difficulty manager
        if (difficultyManager != null)
        {
            // DifficultyManager will handle this in its Update method
        }

        // Check for achievement/milestone triggers
        CheckForMilestones(previousAmount, newAmount);
    }

    public void OnUpgradePurchased(UpgradeData upgrade)
    {
        if (upgrade == null) return;

        PlayerPerks playerPerks = GetPlayerPerks();
        if (playerPerks == null) return;

        // Apply the upgrade to player perks
        playerPerks.ActivateUpgrade(upgrade.UpgradeType);

        // Trigger event system notification
        GameEventSystem.TriggerUpgradePurchased(upgrade);

        // Update all systems that respond to upgrades
        UpdateSystemsForUpgrade(upgrade);

        // Play purchase effects
        PlayUpgradePurchaseEffects(upgrade);
    }

    private void UpdateSystemsForUpgrade(UpgradeData upgrade)
    {
        PlayerPerks playerPerks = GetPlayerPerks();
        if (playerPerks == null) return;

        // Apply positive effects
        ApplyUpgradePositiveEffects(upgrade, playerPerks);
        
        // Apply negative effects (balancing)
        ApplyUpgradeNegativeEffects(upgrade, playerPerks);

        // Update difficulty scaling based on new upgrades
        UpdateCatAIDifficulty();
    }
    
    private void ApplyUpgradePositiveEffects(UpgradeData upgrade, PlayerPerks playerPerks)
    {
        switch (upgrade.UpgradeType)
        {
            case UpgradeType.BuffMagnet:
                if (buffSpawner != null)
                {
                    buffSpawner.EnableMagnetEffectForNewBuffs();
                }
                break;
                
            case UpgradeType.MapExpansion:
                if (collectibleSpawner != null)
                {
                    // Expand spawn area
                }
                break;
                
            case UpgradeType.CheeseMultiplier:
                break;
        }
    }
    
    private void ApplyUpgradeNegativeEffects(UpgradeData upgrade, PlayerPerks playerPerks)
    {
        switch (upgrade.UpgradeType)
        {
            case UpgradeType.SpeedBoost:
                if (hazardSpawner != null)
                {
                    hazardSpawner.SetHazardSpeed(hazardSpawner.GetSpawnInfo().Contains("Speed") ? 
                        ExtractSpeedFromInfo(hazardSpawner.GetSpawnInfo()) * 1.2f : 6f);
                }
                break;
                
            case UpgradeType.Sopa:
                if (difficultyManager != null)
                {
                    difficultyManager.ForceDifficultyIncrease();
                }
                break;
                
            case UpgradeType.Teleport:
                if (hazardSpawner != null)
                {
                    float currentInterval = ExtractIntervalFromInfo(hazardSpawner.GetSpawnInfo());
                    hazardSpawner.SetSpawnInterval(currentInterval * 0.8f);
                }
                break;
                
            case UpgradeType.BuffMagnet:
                if (buffSpawner != null)
                {
                    buffSpawner.ReduceSpawnRate(0.7f);
                }
                break;
                
            case UpgradeType.MapExpansion:
                if (hazardSpawner != null)
                {
                    hazardSpawner.IncreaseSpawnRate(1.3f);
                }
                break;
                
            case UpgradeType.CheeseMultiplier:
                if (difficultyManager != null)
                {
                    difficultyManager.ForceDifficultyIncrease();
                    difficultyManager.ForceDifficultyIncrease();
                }
                break;
                
            case UpgradeType.Invincibility:
                if (hazardSpawner != null)
                {
                    hazardSpawner.IncreaseSpawnRate(2.0f);
                }
                break;
                
            case UpgradeType.SlowMotion:
                break;
        }
    }
    
    private float ExtractSpeedFromInfo(string spawnInfo)
    {
        string[] parts = spawnInfo.Split(',');
        foreach (string part in parts)
        {
            if (part.Trim().StartsWith("Speed:"))
            {
                string speedStr = part.Trim().Substring(6).Trim();
                if (float.TryParse(speedStr, out float speed))
                {
                    return speed;
                }
            }
        }
        return 5f;
    }
    
    private float ExtractIntervalFromInfo(string spawnInfo)
    {
        string[] parts = spawnInfo.Split(',');
        foreach (string part in parts)
        {
            if (part.Trim().StartsWith("Interval:"))
            {
                string intervalStr = part.Trim().Substring(9).Replace("s", "").Trim();
                if (float.TryParse(intervalStr, out float interval))
                {
                    return interval;
                }
            }
        }
        return 2f;
    }

    private void PlayUpgradePurchaseEffects(UpgradeData upgrade)
    {
        AudioManager.Instance?.PlayPurchase();

        PlayerPerks playerPerks = GetPlayerPerks();
        if (playerPerks != null)
        {
            Vector3 playerPosition = playerPerks.transform.position;
            VFXManager.Instance?.PlayBuffActivationEffect(playerPosition);
        }
    }

    private void CheckForMilestones(int previousCheese, int newCheese)
    {
        int[] milestones = { 10, 25, 50, 100, 150, 200, 250, 300 };

        foreach (int milestone in milestones)
        {
            if (previousCheese < milestone && newCheese >= milestone)
            {
                OnMilestoneReached(milestone);
            }
        }
    }

    private void OnMilestoneReached(int milestone)
    {
        GameEventSystem.TriggerMilestoneReached(milestone);
        AudioManager.Instance?.PlayVictory();

        if (milestone >= 100)
        {
            PlayerPerks playerPerks = GetPlayerPerks();
            if (playerPerks != null)
            {
                VFXManager.Instance?.PlayBuffActivationEffect(playerPerks.transform.position);
            }
        }
    }

    public void ChangeGameState(GameState newState)
    {
        if (currentGameState == newState) return;

        GameState previousState = currentGameState;
        currentGameState = newState;

        OnGameStateChanged(previousState, newState);
    }

    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        GameEventSystem.TriggerGameStateChanged(previousState, newState);

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
            case GameState.Market:
            case GameState.GameOver:
            case GameState.Victory:
                Time.timeScale = 0f;
                break;
        }

        NotifySystemsOfStateChange(previousState, newState);
    }

    private void NotifySystemsOfStateChange(GameState previousState, GameState newState)
    {
        if (newState == GameState.GameOver || newState == GameState.Victory)
        {
            hazardSpawner?.StopSpawning();
            buffSpawner?.StopSpawning();
            
            VFXManager.Instance?.StopAllEffects();
            AudioManager.Instance?.StopAllSounds();
        }
        else if (newState == GameState.Playing && previousState != GameState.Playing)
        {
            if (previousState == GameState.Paused || previousState == GameState.Market)
            {
                hazardSpawner?.StartSpawning();
                buffSpawner?.StartSpawning();
            }
        }
    }

    public GameStatistics GetGameStatistics()
    {
        return new GameStatistics
        {
            currentCheese = this.currentCheese,
            gameTime = this.gameTime,
            gameState = this.currentGameState,
            difficultyInfo = difficultyManager?.GetCurrentDifficultyInfo(),
            activeHazards = hazardSpawner?.GetActiveHazardCount() ?? 0,
            activeBuffs = buffSpawner?.GetActiveBuffCount() ?? 0,
            activeMouseHoles = mouseHoleSpawner?.GetActiveHoles()?.Count ?? 0,
            playerUpgrades = GetPlayerPerks()?.GetActiveUpgradeCount() ?? 0
        };
    }
}

[System.Serializable]
public class GameStatistics
{
    public int currentCheese;
    public float gameTime;
    public GameState gameState;
    public DifficultyInfo difficultyInfo;
    public int activeHazards;
    public int activeBuffs;
    public int activeMouseHoles;
    public int playerUpgrades;
}