using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Yeni Input System kullanan PlayerController
/// Kullanım: Bu script'i PlayerController yerine kullanın
/// </summary>
public class PlayerControllerNewInput : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private PlayerPerks playerPerks;

    // Input System Actions
    private PlayerInputActions inputActions;

    // Sprite direction tracking
    private bool facingRight = true; // Varsayılan olarak sağa bakıyor

    // Public properties for external access
    public bool IsInvincible { get; private set; }

    void Awake()
    {
        // Input Actions oluştur
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        // Input Actions'ları aktifleştir
        if (inputActions != null)
        {
            inputActions.Player.Enable();
            // GravityFlip action'ına event bağla
            inputActions.Player.GravityFlip.performed += OnGravityFlip;
        }
    }

    void OnDisable()
    {
        // Input Actions'ları devre dışı bırak
        if (inputActions != null)
        {
            inputActions.Player.Disable();
            // Event'i temizle
            inputActions.Player.GravityFlip.performed -= OnGravityFlip;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerPerks = GetComponent<PlayerPerks>();
        
        if (rb == null)
        {
            Debug.LogError("Player: Rigidbody2D bulunamadı! Lütfen Rigidbody2D component ekleyin.");
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("Player: SpriteRenderer bulunamadı! Lütfen SpriteRenderer component ekleyin.");
        }

        // TextureManager'dan texture uygula
        if (TextureManager.Instance != null)
        {
            TextureManager.Instance.ApplyTextureToNewObject(gameObject, "player");
        }


    }


    void Update()
    {
        // Only process input during gameplay
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameState() != GameState.Playing)
        {
            return;
        }

        // Move action'ından input al
        if (inputActions != null)
        {
            horizontalInput = inputActions.Player.Move.ReadValue<float>();
        }

        // Ability inputs
        HandleAbilityInputs();

        // Sprite yönünü güncelle
        UpdateSpriteDirection();
    }

    void HandleAbilityInputs()
    {
        if (playerPerks == null) return;

        // Sopa ability - E key using New Input System
        if (playerPerks.HasSopa && Keyboard.current.eKey.wasPressedThisFrame)
        {
            playerPerks.UseSopa();
        }
        
        // Teleport ability - Left mouse click using New Input System
        if (playerPerks.CanTeleport && Mouse.current.leftButton.wasPressedThisFrame)
        {
            playerPerks.UseTeleport();
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Yatay hareket with speed multiplier from upgrades
        float currentMoveSpeed = 5f; // Default move speed
        if (playerPerks != null)
        {
            currentMoveSpeed *= playerPerks.SpeedMultiplier;
        }

        // Sadece yatay velocity'yi değiştir, dikey velocity'yi koru
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 newVelocity = new Vector2(horizontalInput * currentMoveSpeed, currentVelocity.y);
        
        // Velocity'yi MovePosition ile değil, doğrudan atayarak ayarla
        rb.linearVelocity = newVelocity;
    }

    void OnGravityFlip(InputAction.CallbackContext context)
    {
        FlipGravity();
    }

    void FlipGravity()
    {
        if (rb == null) return;

        // Only allow gravity flip during gameplay
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameState() != GameState.Playing)
        {
            return;
        }

        // Yerçekimini ters çevir
        rb.gravityScale *= -1f;
        
        // Gravity tersine çevrildi mi kontrol et
        bool isGravityReversed = rb.gravityScale < 0f;



        // Sadece player texture'ını tepe taklak çevir
        if (TextureManager.Instance != null)
        {
            TextureManager.Instance.FlipPlayerTextureOnGravityChange(isGravityReversed);
        }

        // Ses efekti çal
        AudioManager.Instance?.PlayGravityShift();
    }



    /// <summary>
    /// Handles player taking damage and triggers game over
    /// </summary>
    public void TakeDamage()
    {
        if (IsInvincible) return;


        
        // Play death sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMouseDeath();
        }
        
        // Trigger game over (GameManager will handle death effects)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }



    /// <summary>
    /// Activates invincibility for a specific duration (for BuffCollectible compatibility)
    /// </summary>
    public void ActivateInvincibility(float duration)
    {
        SetInvincible(true, duration);
    }

    /// <summary>
    /// Sets invincibility state for temporary buffs
    /// </summary>
    public void SetInvincible(bool invincible, float duration = 0f)
    {
        IsInvincible = invincible;
        
        if (invincible && duration > 0f)
        {
            // Stop any existing invincibility timer
            StopAllCoroutines();
            StartCoroutine(InvincibilityTimer(duration));
            
            // Start visual effect
            if (playerPerks != null)
            {
                playerPerks.StartBuffVisualEffect(duration);
    
            }
        }
        else if (!invincible)
        {
            // Stop visual effect when invincibility ends
            if (playerPerks != null)
            {
                playerPerks.StopBuffVisualEffect();
            }
        }
        

    }

    /// <summary>
    /// Coroutine to handle temporary invincibility
    /// </summary>
    System.Collections.IEnumerator InvincibilityTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        IsInvincible = false;
        
        // Stop visual effect
        if (playerPerks != null)
        {
            playerPerks.StopBuffVisualEffect();
        }
        

    }

    /// <summary>
    /// Updates sprite direction based on movement input (A/D keys)
    /// </summary>
    void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;

        // Sadece hareket input'u varsa yön değiştir
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            bool shouldFaceRight = horizontalInput > 0;

            // Sadece yön değiştiğinde sprite'ı çevir
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                spriteRenderer.flipX = !facingRight; // flipX true olduğunda sola bakar
                

            }
        }
    }
}