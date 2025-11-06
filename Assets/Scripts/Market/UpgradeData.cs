using UnityEngine;

/// <summary>
/// ScriptableObject that defines upgrade properties for the market system
/// </summary>
[CreateAssetMenu(fileName = "New Upgrade", menuName = "Cheese Hunter/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Basic Info")]
    public string UpgradeName = "New Upgrade";
    public Sprite Icon;
    
    [Header("Effects")]
    [TextArea(2, 4)]
    public string PositiveEffectDescription = "Positive effect description";
    
    [TextArea(2, 4)]
    public string NegativeEffectDescription = "Negative effect description";
    
    [Header("Cost")]
    public int CheeseCost = 10;
    
    [Header("Type")]
    public UpgradeType UpgradeType = UpgradeType.SpeedBoost;
}

/// <summary>
/// Enum defining all available upgrade types
/// </summary>
public enum UpgradeType
{
    SpeedBoost,
    Sopa,
    Teleport,
    BuffMagnet,
    MapExpansion,
    CheeseMultiplier,
    Invincibility,
    SlowMotion
}