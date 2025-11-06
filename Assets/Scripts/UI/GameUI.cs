using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// In-game UI controller - handles score display and game state UI
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Victory Panel")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private TextMeshProUGUI victoryHighScoreText;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryMainMenuButton;
    
    [Header("Ability UI")]
    [SerializeField] private GameObject abilityPanel;
    
    void Start()
    {
        SetupUI();
        HideAllPanels();
        SetupAbilityPanel();
    }
    
    void SetupUI()
    {
        // Auto-find UI elements if not assigned in Inspector
        if (scoreText == null)
        {
            GameObject scoreObj = GameObject.Find("ScoreText");
            if (scoreObj != null) scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");
            
        if (victoryPanel == null)
            victoryPanel = GameObject.Find("VictoryPanel");
            
        // Auto-find AbilityPanel
        if (abilityPanel == null)
        {
            abilityPanel = GameObject.Find("AbilityPanel");
        }
            
        // Auto-find game over texts
        if (gameOverScoreText == null && gameOverPanel != null)
        {
            Transform scoreTransform = gameOverPanel.transform.Find("GameOverScoreText");
            if (scoreTransform != null) gameOverScoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
        }
        
        if (gameOverHighScoreText == null && gameOverPanel != null)
        {
            Transform highScoreTransform = gameOverPanel.transform.Find("GameOverHighScoreText");
            if (highScoreTransform != null) gameOverHighScoreText = highScoreTransform.GetComponent<TextMeshProUGUI>();
        }
        
        // Auto-find victory texts
        if (victoryScoreText == null && victoryPanel != null)
        {
            Transform scoreTransform = victoryPanel.transform.Find("VictoryScoreText");
            if (scoreTransform != null) victoryScoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
        }
        
        if (victoryHighScoreText == null && victoryPanel != null)
        {
            Transform highScoreTransform = victoryPanel.transform.Find("VictoryHighScoreText");
            if (highScoreTransform != null) victoryHighScoreText = highScoreTransform.GetComponent<TextMeshProUGUI>();
        }
        
        // Setup button listeners
        SetupButtonListeners();
    }
    
    void SetupButtonListeners()
    {
        // Setup game over buttons
        if (gameOverPanel != null)
        {
            if (restartButton == null)
            {
                Button[] buttons = gameOverPanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.name.Contains("Restart"))
                        restartButton = btn;
                    else if (btn.name.Contains("Menu"))
                        mainMenuButton = btn;
                }
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
            }
            
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() => GameManager.Instance?.LoadMainMenu());
            }
        }
        
        // Setup victory buttons
        if (victoryPanel != null)
        {
            if (victoryRestartButton == null)
            {
                Button[] buttons = victoryPanel.GetComponentsInChildren<Button>(true);
                foreach (Button btn in buttons)
                {
                    if (btn.name.Contains("Restart"))
                        victoryRestartButton = btn;
                    else if (btn.name.Contains("Menu"))
                        victoryMainMenuButton = btn;
                }
            }
            
            if (victoryRestartButton != null)
            {
                victoryRestartButton.onClick.RemoveAllListeners();
                victoryRestartButton.onClick.AddListener(() => GameManager.Instance?.RestartGame());
            }
            
            if (victoryMainMenuButton != null)
            {
                victoryMainMenuButton.onClick.RemoveAllListeners();
                victoryMainMenuButton.onClick.AddListener(() => GameManager.Instance?.LoadMainMenu());
            }
        }
    }
    
    void HideAllPanels()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);

        }
        else
        {
            Debug.LogWarning("GameUI: Game Over panel not found!");
        }
            
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);

        }
        else
        {
            Debug.LogWarning("GameUI: Victory panel not found!");
        }
    }
    
    public void UpdateScore(int cheese)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Peynir: {cheese}";
        }
    }
    
    public void ShowGameOver(int finalScore, int highScore)
    {

        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverScoreText != null)
                gameOverScoreText.text = $"Toplanan Peynir: {finalScore}";
                
            if (gameOverHighScoreText != null)
                gameOverHighScoreText.text = $"En Yüksek Skor: {highScore}";
                

        }
        else
        {
            Debug.LogWarning("GameUI: Game Over panel not found!");
        }
    }
    

    public void ShowVictory(int finalScore, int highScore, float gameTime)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            if (victoryScoreText != null)
            {
                victoryScoreText.text = $"Tebrikler! Fare Deliğine Ulaştınız!\n" +
                                       $"Toplanan Peynir: {finalScore}\n" +
                                       $"Oyun Süresi: {gameTime:F1} saniye";
            }
            
            if (victoryHighScoreText != null)
                victoryHighScoreText.text = $"En Yüksek Skor: {highScore}";
        }
    }
    
    void SetupAbilityPanel()
    {
        if (abilityPanel != null)
        {
            // Sağ alt köşeye yerleştir (Canvas koordinatlarında)
            RectTransform rectTransform = abilityPanel.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = abilityPanel.AddComponent<RectTransform>();
            }
            
            // Anchor'ı sağ alt köşeye ayarla
            rectTransform.anchorMin = new Vector2(1f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 0f);
            rectTransform.pivot = new Vector2(1f, 0f);
            
            // Pozisyonu ayarla (sağdan 50px, alttan 50px)
            rectTransform.anchoredPosition = new Vector2(-100f, 50f);
            

        }
    }
    
    /// <summary>
    /// Refreshes ability UI when player purchases new abilities
    /// </summary>
    public void RefreshAbilityUI()
    {
        // AbilityUI component'i varsa refresh et
        if (abilityPanel != null)
        {
            AbilityUI abilityUI = abilityPanel.GetComponent<AbilityUI>();
            if (abilityUI != null)
            {
                abilityUI.RefreshAbilityUI();
            }
        }
    }


}
