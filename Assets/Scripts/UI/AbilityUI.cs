using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple ability display - shows ability status and cooldowns (visual only)
/// </summary>
public class AbilityUI : MonoBehaviour
{
    [Header("Sopa Ability")]
    [SerializeField] private Image sopaIcon;
    [SerializeField] private TextMeshProUGUI sopaCooldownText;
    
    [Header("Teleport Ability")]
    [SerializeField] private Image teleportIcon;
    [SerializeField] private TextMeshProUGUI teleportCooldownText;
    
    private PlayerPerks playerPerks;
    
    void Start()
    {
        SetupAbilityUI();
        FindPlayerPerks();
    }
    
    void Update()
    {
        UpdateCooldownDisplays();
    }
    
    void SetupAbilityUI()
    {
        // Auto-find UI elements if not assigned
        if (sopaIcon == null)
        {
            GameObject sopaObj = GameObject.Find("SopaIcon");
            if (sopaObj != null) sopaIcon = sopaObj.GetComponent<Image>();
        }
        
        if (teleportIcon == null)
        {
            GameObject teleportObj = GameObject.Find("TeleportIcon");
            if (teleportObj != null) teleportIcon = teleportObj.GetComponent<Image>();
        }
        
        if (sopaCooldownText == null)
        {
            GameObject sopaTextObj = GameObject.Find("SopaCooldownText");
            if (sopaTextObj != null) 
            {
                sopaCooldownText = sopaTextObj.GetComponent<TextMeshProUGUI>();
                if (sopaCooldownText != null)
                {
                    sopaCooldownText.fontSize = 24;
                    sopaCooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                }
            }
        }
        
        if (teleportCooldownText == null)
        {
            GameObject teleportTextObj = GameObject.Find("TeleportCooldownText");
            if (teleportTextObj != null) 
            {
                teleportCooldownText = teleportTextObj.GetComponent<TextMeshProUGUI>();
                if (teleportCooldownText != null)
                {
                    teleportCooldownText.fontSize = 24;
                    teleportCooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                }
            }
        }
        
        // Set initial textures from TextureManager
        if (TextureManager.Instance != null)
        {
            if (sopaIcon != null && TextureManager.Instance.sopaSprite != null)
                sopaIcon.sprite = TextureManager.Instance.sopaSprite;
                
            if (teleportIcon != null && TextureManager.Instance.teleportSprite != null)
                teleportIcon.sprite = TextureManager.Instance.teleportSprite;
        }
    }
    
    void FindPlayerPerks()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPerks = player.GetComponent<PlayerPerks>();
        }
        
        if (playerPerks == null)
        {
            Debug.LogWarning("AbilityUI: PlayerPerks component not found!");
        }
    }
    
    void UpdateCooldownDisplays()
    {
        if (playerPerks == null) return;
        
        // Update Sopa UI
        UpdateSopaUI();
        
        // Update Teleport UI
        UpdateTeleportUI();
    }
    
    void UpdateSopaUI()
    {
        bool hasSopa = playerPerks.HasSopa;
        float remainingCooldown = playerPerks.SopaCooldown - (Time.time - playerPerks.LastSopaTime);
        bool onCooldown = remainingCooldown > 0;
        
        // Update icon transparency
        if (sopaIcon != null)
        {
            Color iconColor = sopaIcon.color;
            if (!hasSopa)
            {
                iconColor.a = 0.3f; // Very transparent if not purchased
            }
            else if (onCooldown)
            {
                iconColor.a = 0.6f; // Semi-transparent if on cooldown
            }
            else
            {
                iconColor.a = 1.0f; // Fully visible if available
            }
            sopaIcon.color = iconColor;
        }
        
        // Update cooldown text
        if (sopaCooldownText != null)
        {
            if (!hasSopa)
            {
                sopaCooldownText.text = "E";
                sopaCooldownText.color = Color.red;
            }
            else if (onCooldown)
            {
                sopaCooldownText.text = Mathf.Ceil(remainingCooldown).ToString();
                sopaCooldownText.color = Color.yellow;
            }
            else
            {
                sopaCooldownText.text = "E";
                sopaCooldownText.color = Color.green;
            }
        }
    }
    
    void UpdateTeleportUI()
    {
        bool canTeleport = playerPerks.CanTeleport;
        float remainingCooldown = playerPerks.TeleportCooldown - (Time.time - playerPerks.LastTeleportTime);
        bool onCooldown = remainingCooldown > 0;
        
        // Update icon transparency
        if (teleportIcon != null)
        {
            Color iconColor = teleportIcon.color;
            if (!canTeleport)
            {
                iconColor.a = 0.3f; // Very transparent if not purchased
            }
            else if (onCooldown)
            {
                iconColor.a = 0.6f; // Semi-transparent if on cooldown
            }
            else
            {
                iconColor.a = 1.0f; // Fully visible if available
            }
            teleportIcon.color = iconColor;
        }
        
        // Update cooldown text
        if (teleportCooldownText != null)
        {
            if (!canTeleport)
            {
                teleportCooldownText.text = "M1";
                teleportCooldownText.color = Color.red;
            }
            else if (onCooldown)
            {
                teleportCooldownText.text = Mathf.Ceil(remainingCooldown).ToString();
                teleportCooldownText.color = Color.yellow;
            }
            else
            {
                teleportCooldownText.text = "M1";
                teleportCooldownText.color = Color.green;
            }
        }
    }
    
    /// <summary>
    /// Called when player purchases an ability to refresh UI
    /// </summary>
    public void RefreshAbilityUI()
    {
        // Force immediate UI update
        UpdateCooldownDisplays();
    }
}