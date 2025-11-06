using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles the market UI for purchasing upgrades
/// </summary>
public class MarketUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject marketPanel;
    [SerializeField] private TextMeshProUGUI cheeseAmountText;
    [SerializeField] private Button[] upgradeButtons = new Button[3];
    [SerializeField] private TextMeshProUGUI[] upgradeTexts = new TextMeshProUGUI[3];
    [SerializeField] private Button closeButton;
    
    [Header("Available Upgrades")]
    [SerializeField] private List<UpgradeData> availableUpgrades = new List<UpgradeData>();
    
    private UpgradeData[] currentOffers = new UpgradeData[3];
    
    void Start()
    {
        SetupUI();
        CreateDefaultUpgrades();
        
        // Find existing market panel - DO NOT CREATE NEW ONE
        if (marketPanel == null)
        {
            marketPanel = GameObject.Find("MarketPanel");
            if (marketPanel == null)
            {
                Debug.LogError("MarketUI: MarketPanel not found in scene! Please assign it in Inspector.");
            }
        }
        
        // Ensure market panel is closed at start
        if (marketPanel != null)
        {
            marketPanel.SetActive(false);
            Debug.Log("MarketUI: Market panel set to inactive at start");
        }
    }
    
void SetupUI()
    {
        // Find existing market panel - DO NOT CREATE NEW ONE
        if (marketPanel == null)
        {
            marketPanel = GameObject.Find("MarketPanel");
            if (marketPanel == null)
            {
                // Try alternative names
                marketPanel = GameObject.Find("Market Panel");
                if (marketPanel == null)
                {
                    marketPanel = GameObject.Find("Market");
                }
            }
            
            if (marketPanel != null)
            {
                Debug.Log($"MarketUI: Found existing MarketPanel: {marketPanel.name}");
            }
            else
            {
                Debug.LogError("MarketUI: MarketPanel not found in scene! Please assign it in Inspector.");
            }
        }
        
        // Auto-find UI components in market panel
        if (marketPanel != null)
        {
            AutoFindUIComponents();
        }
        
        // Auto-find upgrade texts if not assigned
        AutoFindUpgradeTexts();
        
        // Setup button listeners
        SetupButtonListeners();
    }

void AutoFindUIComponents()
    {
        if (marketPanel == null) return;
        
        // Find cheese amount text
        if (cheeseAmountText == null)
        {
            TextMeshProUGUI[] texts = marketPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.ToLower().Contains("cheese") || text.name.ToLower().Contains("peynir"))
                {
                    cheeseAmountText = text;
                    Debug.Log($"MarketUI: Auto-found cheese text: {text.name}");
                    break;
                }
            }
        }
        
        // Find close button
        if (closeButton == null)
        {
            Button[] buttons = marketPanel.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                if (button.name.ToLower().Contains("close") || button.name.ToLower().Contains("kapat") || button.name.ToLower().Contains("exit"))
                {
                    closeButton = button;
                    Debug.Log($"MarketUI: Auto-found close button: {button.name}");
                    break;
                }
            }
        }
        
        // Find upgrade buttons
        if (upgradeButtons[0] == null || upgradeButtons[1] == null || upgradeButtons[2] == null)
        {
            Button[] buttons = marketPanel.GetComponentsInChildren<Button>();
            int upgradeButtonIndex = 0;
            
            foreach (Button button in buttons)
            {
                if (button != closeButton && upgradeButtonIndex < 3)
                {
                    if (button.name.ToLower().Contains("upgrade") || button.name.ToLower().Contains("yukseltme") || 
                        button.name.ToLower().Contains("button") && !button.name.ToLower().Contains("close"))
                    {
                        upgradeButtons[upgradeButtonIndex] = button;
                        Debug.Log($"MarketUI: Auto-found upgrade button {upgradeButtonIndex}: {button.name}");
                        upgradeButtonIndex++;
                    }
                }
            }
        }
    }

    
    void AutoFindUpgradeTexts()
    {
        // Find upgrade texts automatically
        for (int i = 0; i < upgradeTexts.Length; i++)
        {
            if (upgradeTexts[i] == null && upgradeButtons[i] != null)
            {
                // Look for text component in upgrade button children
                TextMeshProUGUI textComponent = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    upgradeTexts[i] = textComponent;
                    Debug.Log($"MarketUI: Auto-found upgrade text {i}: {textComponent.name}");
                }
            }
        }
    }
    

    
    void SetupButtonListeners()
    {
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMarket);
            Debug.Log("MarketUI: Close button listener added");
        }
        
        // Setup upgrade buttons
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i; // Capture for closure
            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].onClick.AddListener(() => PurchaseUpgrade(index));
                Debug.Log($"MarketUI: Upgrade button {i} listener added (dynamic)");
            }
        }
    }
    

    
    void CreateDefaultUpgrades()
    {
        // Load upgrades from Assets/Upgrades folder
        LoadUpgradesFromAssets();
        
        // Fallback: Create basic upgrades if loading failed
        if (availableUpgrades.Count == 0)
        {
            Debug.LogWarning("MarketUI: No upgrades loaded from assets, creating fallback upgrades");
            availableUpgrades.Add(CreateUpgrade("Hız Artışı", "Daha hızlı hareket et", "Kediler de hızlanır", 10, UpgradeType.SpeedBoost));
            availableUpgrades.Add(CreateUpgrade("Sopa", "Kedileri iterek", "Kediler daha iyi nişan alır", 15, UpgradeType.Sopa));
            availableUpgrades.Add(CreateUpgrade("Işınlanma", "Kısa mesafe ışınlanma", "Kediler takip eder", 20, UpgradeType.Teleport));
            availableUpgrades.Add(CreateUpgrade("Buff Mıknatısı", "Buff'ları çeker", "Daha az buff spawn olur", 25, UpgradeType.BuffMagnet));
            availableUpgrades.Add(CreateUpgrade("Harita Genişletme", "Daha geniş alan", "Daha fazla kedi", 30, UpgradeType.MapExpansion));
        }
    }
    
    void LoadUpgradesFromAssets()
    {
        // Load all UpgradeData assets from Resources or direct references
        UpgradeData[] loadedUpgrades = Resources.LoadAll<UpgradeData>("Upgrades");
        
        if (loadedUpgrades.Length > 0)
        {
            availableUpgrades.AddRange(loadedUpgrades);
            Debug.Log($"MarketUI: Loaded {loadedUpgrades.Length} upgrades from assets");
        }
        else
        {
            // Try loading specific upgrades by name
            string[] upgradeNames = { "SpeedBoostUpgrade", "SopaUpgrade", "TeleportUpgrade", "BuffMagnetUpgrade", "MapExpansionUpgrade" };
            
            foreach (string upgradeName in upgradeNames)
            {
                UpgradeData upgrade = Resources.Load<UpgradeData>($"Upgrades/{upgradeName}");
                if (upgrade != null)
                {
                    availableUpgrades.Add(upgrade);
                    Debug.Log($"MarketUI: Loaded upgrade: {upgrade.UpgradeName}");
                }
            }
        }
    }
    
    UpgradeData CreateUpgrade(string name, string positive, string negative, int cost, UpgradeType type)
    {
        UpgradeData upgrade = ScriptableObject.CreateInstance<UpgradeData>();
        upgrade.UpgradeName = name;
        upgrade.PositiveEffectDescription = positive;
        upgrade.NegativeEffectDescription = negative;
        upgrade.CheeseCost = cost;
        upgrade.UpgradeType = type;
        return upgrade;
    }
    
    public void ShowMarket(List<UpgradeData> upgrades, MarketTrigger marketTrigger)
    {
        if (marketPanel != null)
        {
            marketPanel.SetActive(true);
            
            // Always generate fresh offers based on current player state
            // This ensures purchased upgrades don't appear again
            GenerateRandomOffers();
            
            UpdateUI();
            Debug.Log("MarketUI: Market shown successfully with fresh offers");
        }
        else
        {
            Debug.LogError("MarketUI: marketPanel is null! Cannot show market.");
        }
    }
    
    public void OpenMarket()
    {
        Debug.Log("MarketUI: OpenMarket() called");
        
        // Play market open sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMarketOpen();
        }
        
        // Pause game first
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMarketState();
            Debug.Log("MarketUI: Game paused for market");
        }
        
        // Use existing Unity MarketPanel - DO NOT CREATE NEW ONE
        if (marketPanel != null)
        {
            marketPanel.SetActive(true);
            Debug.Log("MarketUI: Market panel activated");
            
            // Generate fresh offers based on current player state
            GenerateRandomOffers();
            
            // Update UI with current data
            UpdateUI();
            
            // Start coroutine to close market after 8 seconds for testing
            StartCoroutine(AutoCloseMarket());
        }
        else
        {
            Debug.LogError("MarketUI: marketPanel is null! Please assign MarketPanel in Inspector.");
        }
    }
    

    
    System.Collections.IEnumerator AutoCloseMarket()
    {
        yield return new WaitForSecondsRealtime(8f); // Use realtime since game is paused
        Debug.Log("MarketUI: Auto-closing market after 5 seconds");
        CloseMarket();
    }
    
    public void CloseMarket()
    {
        Debug.Log("MarketUI: CloseMarket() called");
        
        if (marketPanel != null)
        {
            marketPanel.SetActive(false);
            Debug.Log("MarketUI: Market panel deactivated");
        }
        
        // Resume game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
            Debug.Log("MarketUI: Game resume requested");
        }
    }
    
    void GenerateRandomOffers()
    {
        // Get player perks to filter out already owned upgrades
        PlayerPerks playerPerks = null;
        if (GameManager.Instance != null)
        {
            playerPerks = GameManager.Instance.GetPlayerPerks();
        }
        
        List<UpgradeData> availableForPurchase = new List<UpgradeData>();
        
        foreach (UpgradeData upgrade in availableUpgrades)
        {
            // Check if player already has this upgrade
            bool alreadyOwned = playerPerks != null && playerPerks.HasUpgrade(upgrade.UpgradeType);
            
            if (!alreadyOwned)
            {
                availableForPurchase.Add(upgrade);
                Debug.Log($"MarketUI: {upgrade.UpgradeName} available for purchase");
            }
            else
            {
                Debug.Log($"MarketUI: {upgrade.UpgradeName} already owned, skipping");
            }
        }
        
        Debug.Log($"MarketUI: {availableForPurchase.Count} upgrades available for purchase");
        
        // Clear current offers first
        for (int i = 0; i < 3; i++)
        {
            currentOffers[i] = null;
        }
        
        // Select 3 random upgrades from available ones
        for (int i = 0; i < 3; i++)
        {
            if (availableForPurchase.Count > 0)
            {
                int randomIndex = Random.Range(0, availableForPurchase.Count);
                currentOffers[i] = availableForPurchase[randomIndex];
                availableForPurchase.RemoveAt(randomIndex);
                Debug.Log($"MarketUI: Offer {i}: {currentOffers[i].UpgradeName}");
            }
            else
            {
                currentOffers[i] = null;
                Debug.Log($"MarketUI: No more upgrades available for offer slot {i}");
            }
        }
    }
    
void UpdateUI()
    {
        Debug.Log("MarketUI: UpdateUI() called");
        
        // Generate offers if not already done
        if (currentOffers[0] == null && currentOffers[1] == null && currentOffers[2] == null)
        {
            GenerateRandomOffers();
            Debug.Log("MarketUI: Generated random offers");
        }
        
        // Update cheese amount
        if (cheeseAmountText != null && GameManager.Instance != null)
        {
            cheeseAmountText.text = "Peynir: " + GameManager.Instance.GetCurrentCheese();
            Debug.Log($"MarketUI: Updated cheese text: {cheeseAmountText.text}");
        }
        else
        {
            Debug.LogWarning("MarketUI: cheeseAmountText is null or GameManager not found");
        }
        
        // Update upgrade buttons with enhanced formatting
        for (int i = 0; i < 3; i++)
        {
            if (currentOffers[i] != null)
            {
                string buttonText = FormatUpgradeText(currentOffers[i]);
                
                // Try to find text component if not assigned
                if (upgradeTexts[i] == null && upgradeButtons[i] != null)
                {
                    upgradeTexts[i] = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                }
                
                if (upgradeTexts[i] != null)
                {
                    upgradeTexts[i].text = buttonText;
                    Debug.Log($"MarketUI: Updated upgrade {i} text: {currentOffers[i].UpgradeName}");
                }
                else
                {
                    Debug.LogWarning($"MarketUI: upgradeTexts[{i}] is null");
                }
                
                // Enable/disable button based on affordability
                if (upgradeButtons[i] != null)
                {
                    bool canAfford = GameManager.Instance != null && 
                                   GameManager.Instance.CanAfford(currentOffers[i].CheeseCost);
                    upgradeButtons[i].interactable = canAfford;
                    
                    // Change button color based on affordability
                    var colors = upgradeButtons[i].colors;
                    colors.normalColor = canAfford ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(0.8f, 0.2f, 0.2f, 1f);
                    colors.highlightedColor = canAfford ? new Color(0.3f, 0.9f, 0.3f, 1f) : new Color(0.9f, 0.3f, 0.3f, 1f);
                    upgradeButtons[i].colors = colors;
                    
                    Debug.Log($"MarketUI: Upgrade {i} button interactable: {canAfford}");
                }
            }
            else
            {
                // Try to find text component if not assigned
                if (upgradeTexts[i] == null && upgradeButtons[i] != null)
                {
                    upgradeTexts[i] = upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                }
                
                if (upgradeTexts[i] != null)
                {
                    upgradeTexts[i].text = "Yükseltme Yok";
                }
                
                if (upgradeButtons[i] != null)
                {
                    upgradeButtons[i].interactable = false;
                    
                    // Gray out unavailable buttons
                    var colors = upgradeButtons[i].colors;
                    colors.normalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                    colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                    upgradeButtons[i].colors = colors;
                }
                
                Debug.Log($"MarketUI: No offer for upgrade slot {i}");
            }
        }
        
        Debug.Log("MarketUI: UpdateUI() completed");
    }

string FormatUpgradeText(UpgradeData upgrade)
    {
        string formattedText = $"<size=16><b>{upgrade.UpgradeName}</b></size>\n";
        
        // Add buff/positive effect with green color
        formattedText += $"<color=#00FF00><size=13><b>✓ {upgrade.PositiveEffectDescription}</b></size></color>\n";
        
        // Add risk/negative effect with red color
        formattedText += $"<color=#FF4444><size=13><b>⚠ {upgrade.NegativeEffectDescription}</b></size></color>\n";
        
        // Add cost with yellow color
        formattedText += $"<color=#FFD700><size=14><b>Maliyet: {upgrade.CheeseCost} Peynir</b></size></color>";
        
        return formattedText;
    }

    
    void PurchaseUpgrade(int index)
    {
        if (currentOffers[index] == null || GameManager.Instance == null) return;
        
        UpgradeData upgrade = currentOffers[index];
        
        if (GameManager.Instance.SpendCheese(upgrade.CheeseCost))
        {
            // Apply upgrade
            GameManager.Instance.OnUpgradePurchased(upgrade);
            
            // Refresh ability UI if it's a Sopa or Teleport upgrade
            if (upgrade.UpgradeType == UpgradeType.Sopa || upgrade.UpgradeType == UpgradeType.Teleport)
            {
                GameUI gameUI = FindFirstObjectByType<GameUI>();
                if (gameUI != null)
                {
                    gameUI.RefreshAbilityUI();
                }
            }
            
            Debug.Log($"MarketUI: Purchased {upgrade.UpgradeName}");
            
            // Close market after purchase to prevent multiple purchases
            CloseMarket();
        }
        else
        {
            Debug.Log("MarketUI: Not enough cheese!");
        }
    }
}