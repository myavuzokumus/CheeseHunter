using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// MainMenu UI'ını düzelten Editor script - Menü: Tools > Fix MainMenu UI
/// </summary>
public class FixMainMenuUI : EditorWindow
{
    [MenuItem("Tools/Fix MainMenu UI")]
    static void FixMainMenu()
    {
        // MainMenu sahnesini aç
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        
        // Unity'nin built-in white sprite'ını al
        Sprite whiteSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        
        // Canvas'ı bul ve ayarla
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }
        
        // Canvas Scaler'ı ayarla
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            EditorUtility.SetDirty(scaler);
            Debug.Log("Canvas Scaler configured for 1920x1080!");
        }
        
        // Arka plan oluştur
        GameObject bgObj = GameObject.Find("Background");
        if (bgObj == null)
        {
            bgObj = new GameObject("Background");
            bgObj.transform.SetParent(canvas.transform, false);
            
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = whiteSprite;
            bgImage.color = new Color(0.2f, 0.3f, 0.4f, 1f); // Mavi-gri
            
            bgObj.transform.SetAsFirstSibling(); // En arkada
        }
        
        // TitleText'i ayarla
        GameObject titleObj = GameObject.Find("TitleText");
        if (titleObj != null)
        {
            TextMeshProUGUI txt = titleObj.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = "PEYNİR AVCISI";
                txt.fontSize = 72;
                txt.color = Color.yellow;
                txt.alignment = TextAlignmentOptions.Center;
                
                RectTransform rect = titleObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.7f);
                rect.anchorMax = new Vector2(0.5f, 0.7f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(800, 100);
                
                EditorUtility.SetDirty(titleObj);
            }
        }
        
        // PlayButton'ı ayarla
        GameObject playBtn = GameObject.Find("PlayButton");
        if (playBtn != null)
        {
            Image btnImage = playBtn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.sprite = whiteSprite;
                btnImage.color = new Color(0.3f, 0.6f, 0.3f, 1f); // Yeşil
            }
            
            RectTransform rect = playBtn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(300, 80);
            
            // Button text
            TextMeshProUGUI btnText = playBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = "OYNA";
                btnText.fontSize = 36;
                btnText.color = Color.white;
            }
            
            EditorUtility.SetDirty(playBtn);
        }
        
        // QuitButton'ı ayarla
        GameObject quitBtn = GameObject.Find("QuitButton");
        if (quitBtn != null)
        {
            Image btnImage = quitBtn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.sprite = whiteSprite;
                btnImage.color = new Color(0.6f, 0.3f, 0.3f, 1f); // Kırmızı
            }
            
            RectTransform rect = quitBtn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.35f);
            rect.anchorMax = new Vector2(0.5f, 0.35f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(300, 80);
            
            // Button text
            TextMeshProUGUI btnText = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
            {
                btnText.text = "ÇIKIŞ";
                btnText.fontSize = 36;
                btnText.color = Color.white;
            }
            
            EditorUtility.SetDirty(quitBtn);
        }
        
        // HighScoreText'i ayarla
        GameObject hsObj = GameObject.Find("HighScoreText");
        if (hsObj != null)
        {
            TextMeshProUGUI txt = hsObj.GetComponent<TextMeshProUGUI>();
            if (txt != null)
            {
                int highScore = PlayerPrefs.GetInt("HighScore", 0);
                txt.text = $"En Yüksek Skor: {highScore}";
                txt.fontSize = 36;
                txt.color = Color.white;
                txt.alignment = TextAlignmentOptions.Center;
                
                RectTransform rect = hsObj.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.2f);
                rect.anchorMax = new Vector2(0.5f, 0.2f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(600, 50);
                
                EditorUtility.SetDirty(hsObj);
            }
        }
        
        // Sahneyi kaydet
        EditorSceneManager.SaveOpenScenes();
        
        Debug.Log("MainMenu UI fixed!");
        EditorUtility.DisplayDialog("MainMenu Fixed!", "MainMenu UI configured successfully!", "OK");
    }
}
