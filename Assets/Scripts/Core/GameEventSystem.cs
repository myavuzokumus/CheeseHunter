using UnityEngine;
using System;

/// <summary>
/// Centralized event system for game-wide communication
/// </summary>
public class GameEventSystem : MonoBehaviour
{
    public static GameEventSystem Instance { get; private set; }
    
    // Events
    public static event Action<int, int> OnCheeseChanged;
    public static event Action<UpgradeData> OnUpgradePurchased;
    public static event Action<int> OnMilestoneReached;
    public static event Action<GameState, GameState> OnGameStateChanged;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Only DontDestroyOnLoad if this is a root object
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void TriggerCheeseChanged(int previousAmount, int newAmount)
    {
        OnCheeseChanged?.Invoke(previousAmount, newAmount);
    }
    
    public static void TriggerUpgradePurchased(UpgradeData upgrade)
    {
        OnUpgradePurchased?.Invoke(upgrade);
    }
    
    public static void TriggerMilestoneReached(int milestone)
    {
        OnMilestoneReached?.Invoke(milestone);
    }
    
    public static void TriggerGameStateChanged(GameState previousState, GameState newState)
    {
        OnGameStateChanged?.Invoke(previousState, newState);
    }
}

/// <summary>
/// Game state enumeration
/// </summary>
public enum GameState
{
    Playing,
    Paused,
    Market,
    GameOver,
    Victory
}