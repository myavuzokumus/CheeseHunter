using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main menu UI controller - handles all main menu interactions
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    
    void Start()
    {
        SetupUI();
        UpdateHighScore();
    }
    
    void SetupUI()
    {
        // Auto-find UI elements if not assigned
        if (playButton == null)
        {
            GameObject playObj = GameObject.Find("PlayButton");
            if (playObj != null) playButton = playObj.GetComponent<Button>();
        }
        
        if (quitButton == null)
        {
            GameObject quitObj = GameObject.Find("QuitButton");
            if (quitObj != null) quitButton = quitObj.GetComponent<Button>();
        }
        
        if (titleText == null)
        {
            GameObject titleObj = GameObject.Find("TitleText");
            if (titleObj != null) titleText = titleObj.GetComponent<TextMeshProUGUI>();
        }
        
        if (highScoreText == null)
        {
            GameObject highScoreObj = GameObject.Find("HighScoreText");
            if (highScoreObj != null) highScoreText = highScoreObj.GetComponent<TextMeshProUGUI>();
        }
        
        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PlayGame);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }
        
        // Set title
        if (titleText != null)
        {
            titleText.text = "PEYNİR AVCISI";
        }
    }
    
    void UpdateHighScore()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"En Yüksek Skor: {highScore}";
        }
    }
    
    public void PlayGame()
    {

        SceneManager.LoadScene(gameSceneName);
    }
    
    public void QuitGame()
    {

        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
