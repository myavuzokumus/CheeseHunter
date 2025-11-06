using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class FixUISprites : EditorWindow
{
    [MenuItem("Tools/Fix UI Sprites")]
    static void FixAllUISprites()
    {
        Sprite whiteSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
            }
        }
        
        // Main Camera arka plan
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.backgroundColor = new Color(0.15f, 0.25f, 0.35f, 1f);
            EditorUtility.SetDirty(mainCam);
        }
        
        // ScoreText
        GameObject scoreText = GameObject.Find("ScoreText");
        if (scoreText != null)
        {
            TextMeshProUGUI txt = scoreText.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.fontSize = 48;
                txt.color = Color.white;
                txt.alignment = TextAlignmentOptions.Center;
                txt.text = "Peynir: 0";
                
                RectTransform rect = scoreText.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -20);
                rect.sizeDelta = new Vector2(500, 80);
                
                EditorUtility.SetDirty(scoreText);
            }
        }
        
        // GameOverPanel
        GameObject gameOverPanel = GameObject.Find("GameOverPanel");
        if (gameOverPanel != null)
        {
            Image panelImage = gameOverPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = whiteSprite;
                panelImage.color = new Color(0, 0, 0, 0.95f);
                
                RectTransform rect = gameOverPanel.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                EditorUtility.SetDirty(gameOverPanel);
            }
            
            // GameOverScoreText
            Transform scoreTransform = gameOverPanel.transform.Find("GameOverScoreText");
            if (scoreTransform != null)
            {
                TextMeshProUGUI txt = scoreTransform.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.fontSize = 60;
                    txt.color = Color.white;
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.text = "GAME OVER";
                    
                    RectTransform txtRect = scoreTransform.GetComponent<RectTransform>();
                    txtRect.anchorMin = new Vector2(0.5f, 0.65f);
                    txtRect.anchorMax = new Vector2(0.5f, 0.65f);
                    txtRect.anchoredPosition = Vector2.zero;
                    txtRect.sizeDelta = new Vector2(800, 100);
                    
                    EditorUtility.SetDirty(scoreTransform.gameObject);
                }
            }
            
            // GameOverHighScoreText
            Transform highScoreTransform = gameOverPanel.transform.Find("GameOverHighScoreText");
            if (highScoreTransform != null)
            {
                TextMeshProUGUI txt = highScoreTransform.GetComponent<TextMeshProUGUI>();
                if (txt != null)
                {
                    txt.fontSize = 36;
                    txt.color = Color.yellow;
                    txt.alignment = TextAlignmentOptions.Center;
                    txt.text = "En Yüksek Skor: 0";
                    
                    RectTransform txtRect = highScoreTransform.GetComponent<RectTransform>();
                    txtRect.anchorMin = new Vector2(0.5f, 0.52f);
                    txtRect.anchorMax = new Vector2(0.5f, 0.52f);
                    txtRect.anchoredPosition = Vector2.zero;
                    txtRect.sizeDelta = new Vector2(600, 80);
                    
                    EditorUtility.SetDirty(highScoreTransform.gameObject);
                }
            }
            
            // RestartButton
            Transform restartTransform = gameOverPanel.transform.Find("RestartButton");
            if (restartTransform != null)
            {
                Image btnImage = restartTransform.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.sprite = whiteSprite;
                    btnImage.color = new Color(0.3f, 0.6f, 0.3f, 1f);
                }
                
                RectTransform btnRect = restartTransform.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.5f, 0.38f);
                btnRect.anchorMax = new Vector2(0.5f, 0.38f);
                btnRect.anchoredPosition = Vector2.zero;
                btnRect.sizeDelta = new Vector2(350, 80);
                
                TextMeshProUGUI btnText = restartTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText == null)
                {
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(restartTransform, false);
                    btnText = textObj.AddComponent<TextMeshProUGUI>();
                    
                    RectTransform textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                }
                
                if (btnText != null)
                {
                    btnText.text = "YENİDEN BAŞLA";
                    btnText.fontSize = 32;
                    btnText.color = Color.white;
                    btnText.alignment = TextAlignmentOptions.Center;
                }
                
                EditorUtility.SetDirty(restartTransform.gameObject);
            }
            
            // MenuButton
            Transform menuTransform = gameOverPanel.transform.Find("MenuButton");
            if (menuTransform != null)
            {
                Image btnImage = menuTransform.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.sprite = whiteSprite;
                    btnImage.color = new Color(0.6f, 0.3f, 0.3f, 1f);
                }
                
                RectTransform btnRect = menuTransform.GetComponent<RectTransform>();
                btnRect.anchorMin = new Vector2(0.5f, 0.28f);
                btnRect.anchorMax = new Vector2(0.5f, 0.28f);
                btnRect.anchoredPosition = Vector2.zero;
                btnRect.sizeDelta = new Vector2(350, 80);
                
                TextMeshProUGUI btnText = menuTransform.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText == null)
                {
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(menuTransform, false);
                    btnText = textObj.AddComponent<TextMeshProUGUI>();
                    
                    RectTransform textRect = textObj.GetComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                }
                
                if (btnText != null)
                {
                    btnText.text = "ANA MENÜ";
                    btnText.fontSize = 32;
                    btnText.color = Color.white;
                    btnText.alignment = TextAlignmentOptions.Center;
                }
                
                EditorUtility.SetDirty(menuTransform.gameObject);
            }
            
            // Paneli başlangıçta gizle
            gameOverPanel.SetActive(false);
        }
        
        // VictoryPanel
        GameObject victoryPanel = GameObject.Find("VictoryPanel");
        if (victoryPanel != null)
        {
            Image panelImage = victoryPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = whiteSprite;
                panelImage.color = new Color(0, 0.4f, 0, 0.95f);
                
                RectTransform rect = victoryPanel.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                EditorUtility.SetDirty(victoryPanel);
            }
            
            victoryPanel.SetActive(false);
        }
        
        // MarketPanel
        GameObject marketPanel = GameObject.Find("MarketPanel");
        if (marketPanel != null)
        {
            Image panelImage = marketPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.sprite = whiteSprite;
                panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
                
                RectTransform rect = marketPanel.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.2f, 0.2f);
                rect.anchorMax = new Vector2(0.8f, 0.8f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                EditorUtility.SetDirty(marketPanel);
            }
            
            marketPanel.SetActive(false);
        }
        
        Debug.Log("All UI fixed!");
        EditorUtility.DisplayDialog("UI Fixed!", "GameScene UI configured successfully!\n\n- Canvas: Screen Space Overlay\n- Resolution: 1920x1080\n- Camera: Dark blue-grey\n- All panels positioned", "OK");
    }
}
