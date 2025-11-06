using UnityEngine;

/// <summary>
/// Manages loading and providing textures/sprites for the game
/// </summary>
public class TextureManager : MonoBehaviour
{
    public static TextureManager Instance { get; private set; }
    
    [Header("Game Sprites")]
    public Sprite mouseSprite;
    public Sprite cheeseSprite;
    public Sprite catSprite;
    public Sprite buffSprite;
    public Sprite marketSprite;
    public Sprite mouseHoleSprite;
    public Sprite wallSprite;
    public Sprite sopaSprite;
    public Sprite teleportSprite;
    public Sprite uiButtonSprite;
    public Sprite marketBackgroundSprite;
    public Sprite sopaVfxSprite;
    public Sprite mainMenuBackgroundSprite;
    
    [Header("Company Branding")]
    public Sprite companyLogo;
    
    [Header("Gravity Flip Settings")]
    [SerializeField] private bool flipTexturesOnGravityChange = true;
    

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllTextures();
            
            // Texture yüklendikten sonra tüm objelere uygula
            Invoke("ApplyAllTextures", 0.5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void LoadAllTextures()
    {
        // SADECE COMPONENT'TE ATANAN SPRITE'LAR KULLANILIR
        Debug.Log("TextureManager: Using ONLY component-assigned sprites!");
        Debug.Log($"TextureManager: mouseSprite={mouseSprite?.name}, cheeseSprite={cheeseSprite?.name}, catSprite={catSprite?.name}");
        Debug.Log($"TextureManager: sopaSprite={sopaSprite?.name}, teleportSprite={teleportSprite?.name}, sopaVfxSprite={sopaVfxSprite?.name}");
    }
    

    

    

    
    /// <summary>
    /// MERKEZI TEXTURE UYGULAMA SİSTEMİ - TÜM OBJELERE TEXTURE UYGULA
    /// </summary>
    public void ApplyAllTextures()
    {
        Debug.Log("TextureManager: Applying textures to ALL objects...");
        
        // Duvarlara texture uygula
        ApplyWallTextures();
        
        // Mevcut objelere texture uygula
        ApplyTextureToExistingObjects();
        
        Debug.Log("TextureManager: All textures applied!");
    }
    
    void ApplyWallTextures()
    {
        GameObject[] walls = {
            GameObject.Find("TopWall"),
            GameObject.Find("BottomWall"), 
            GameObject.Find("LeftWall"),
            GameObject.Find("RightWall")
        };
        
        foreach (GameObject wall in walls)
        {
            if (wall != null)
            {
                SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
                if (sr != null && wallSprite != null)
                {
                    sr.sprite = wallSprite;
                    sr.color = Color.white;
                    sr.drawMode = SpriteDrawMode.Tiled;
                    
                    // DUVARLAR İÇİN SCALE DEĞİŞTİRME! Collider'ları bozar
                    // Duvarların orijinal scale'ini koru
                    Debug.Log($"TextureManager: Applied wall texture to {wall.name} (scale preserved)");
                }
            }
        }
    }
    
    void ApplyTextureToExistingObjects()
    {
        // Player'a texture uygula
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            ApplyTextureToObject(player, mouseSprite);
        }
        
        // Collectible'lara texture uygula
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (GameObject obj in collectibles)
        {
            ApplyTextureToObject(obj, cheeseSprite);
        }
        
        // Hazard'lara texture uygula
        GameObject[] hazards = GameObject.FindGameObjectsWithTag("Hazard");
        foreach (GameObject obj in hazards)
        {
            ApplyTextureToObject(obj, catSprite);
        }
        
        // Buff'lara texture uygula
        BuffCollectible[] buffs = FindObjectsByType<BuffCollectible>(FindObjectsSortMode.None);
        foreach (BuffCollectible buff in buffs)
        {
            ApplyTextureToObject(buff.gameObject, buffSprite);
        }
        
        // Market'lara texture uygula
        MarketTrigger[] markets = FindObjectsByType<MarketTrigger>(FindObjectsSortMode.None);
        foreach (MarketTrigger market in markets)
        {
            ApplyTextureToObject(market.gameObject, marketSprite);
        }
    }
    
    /// <summary>
    /// Objeye sadece texture uygula - scale component'te ayarlanır
    /// </summary>
    public void ApplyTextureToObject(GameObject obj, Sprite sprite)
    {
        if (obj == null || sprite == null) return;
        
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = sprite;
            sr.color = Color.white;
            
            Debug.Log($"TextureManager: Applied texture to {obj.name} (scale preserved)");
        }
    }
    
    /// <summary>
    /// SADECE COMPONENT'TE ATANAN SPRITE'LARI KULLANIR - Scale component'te ayarlanır
    /// </summary>
    public void ApplyTextureToNewObject(GameObject obj, string objectType)
    {
        Sprite targetSprite = null;
        
        switch (objectType.ToLower())
        {
            case "player":
                targetSprite = mouseSprite;
                break;
            case "collectible":
            case "cheese":
                targetSprite = cheeseSprite;
                break;
            case "hazard":
            case "cat":
                targetSprite = catSprite;
                break;
            case "buff":
                targetSprite = buffSprite;
                break;
            case "market":
                targetSprite = marketSprite;
                break;
        }
        
        if (targetSprite != null)
        {
            ApplyTextureToObject(obj, targetSprite);
            
            // Yeni spawn olan objeye mevcut gravity durumuna göre flip uygula
            ApplyCurrentGravityFlipToNewObject(obj);
        }
        else
        {
            Debug.LogWarning($"TextureManager: No sprite assigned for {objectType} in component!");
        }
    }
    
    /// <summary>
    /// Yeni spawn olan objeye mevcut gravity durumuna göre texture flip uygula (sadece player için)
    /// </summary>
    private void ApplyCurrentGravityFlipToNewObject(GameObject obj)
    {
        if (!flipTexturesOnGravityChange) return;
        
        // Sadece player objesi için gravity flip uygula
        if (obj.CompareTag("Player"))
        {
            // Player'ın gravity durumunu kontrol et
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    bool isGravityReversed = playerRb.gravityScale < 0f;
                    FlipObjectTexture(obj, isGravityReversed);
                    
                    Debug.Log($"TextureManager: Applied current gravity flip to player - FlipY: {isGravityReversed}");
                }
            }
        }
    }
    
    /// <summary>
    /// Gravity değiştiğinde sadece player texture'ını tepe taklak çevir
    /// </summary>
    public void FlipPlayerTextureOnGravityChange(bool isGravityReversed)
    {
        if (!flipTexturesOnGravityChange) return;
        
        Debug.Log($"TextureManager: Flipping player texture - Gravity reversed: {isGravityReversed}");
        
        // Sadece Player texture'ını çevir
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FlipObjectTexture(player, isGravityReversed);
        }
    }
    
    /// <summary>
    /// Tek bir objenin texture'ını Y ekseninde çevir
    /// </summary>
    private void FlipObjectTexture(GameObject obj, bool flipY)
    {
        if (obj == null) return;
        
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipY = flipY;
            Debug.Log($"TextureManager: Flipped texture for {obj.name} - FlipY: {flipY}");
        }
    }
}