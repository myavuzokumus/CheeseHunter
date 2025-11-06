using UnityEngine;

/// <summary>
/// Enhanced Cat AI that responds to difficulty scaling with chance-based abilities
/// Implements balanced risk-reward mechanics coordinated with DifficultyManager
/// </summary>
public class Cat : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float currentSpeed = 5f;

    [SerializeField] private float speedMultiplier = 1f;

    [Header("Bounce Feature")]
    [SerializeField] private bool canBounce = false;
    [SerializeField] private bool aimOnBounce = false; // No default aiming
    [SerializeField] private int maxBounces = 1; // Default max bounces

    [SerializeField] private float bounceChance = 0f; // No default bounce chance
    private int bounceCount = 0;
    private bool hasDecidedBounce = false; // Bounce kararı verildi mi?

    [Header("Enhanced AI")]
    [SerializeField] private bool enablePursuit = false; // Enhanced pursuit behavior
    [SerializeField] private float pursuitStrength = 0.3f; // How much to adjust trajectory towards player
    [SerializeField] private float pursuitUpdateInterval = 0.5f; // How often to update pursuit
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Rigidbody2D rb;
    private Transform playerTransform;
    private float lastPursuitUpdate = 0f;
    private bool facingRight = true; // Varsayılan olarak sağa bakıyor

void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpeed = baseSpeed * speedMultiplier;

        // Find player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // REMOVE DEFAULT ABILITIES - All abilities are now chance-based and score-dependent
        canBounce = false;
        aimOnBounce = false;
        enablePursuit = false;
        bounceChance = 0f;
        speedMultiplier = 1f;

        // Apply chance-based abilities based on cheese score
        ApplyChanceBasedAbilities();

        // Configure rigidbody for horizontal movement only
        if (rb != null)
        {
            rb.gravityScale = 0f; // No gravity
            rb.linearDamping = 0f; // No air resistance
            rb.angularDamping = 0f; // No rotation resistance
            rb.freezeRotation = true; // Prevent rotation
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Better collision detection
            
            // Initial velocity will be set by SetSpeed() method called from spawner
        }

        // Texture yönünü başlangıç hızına göre ayarla
        UpdateSpriteDirection();


    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player ile çarpışma kontrolü
        if (other.CompareTag("Player"))
        {
            PlayerControllerNewInput player = other.GetComponent<PlayerControllerNewInput>();
            if (player != null && !player.IsInvincible)
            {
                // Player'a hasar ver
                player.TakeDamage();
            }
            return;
        }
        
        // Kedi-kedi çarpışması kontrolü (trigger için)
        Cat otherCat = other.GetComponent<Cat>();
        if (other.CompareTag("Hazard") || otherCat != null)
        {

            
            // Her iki kedinin de physics'ini anında durdur
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = false;
            }
            
            // Diğer kedinin physics'ini de durdur
            Rigidbody2D otherRb = other.GetComponent<Rigidbody2D>();
            if (otherRb != null)
            {
                otherRb.linearVelocity = Vector2.zero;
                otherRb.angularVelocity = 0f;
                otherRb.bodyType = RigidbodyType2D.Kinematic;
                otherRb.simulated = false;
            }
            
            // Collider'ları devre dışı bırak
            Collider2D thisCollider = GetComponent<Collider2D>();
            if (thisCollider != null) thisCollider.enabled = false;
            if (other != null) other.enabled = false;
            
            // İkisini de anında yok et
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // Enhanced pursuit behavior
        if (enablePursuit && playerTransform != null && Time.time - lastPursuitUpdate > pursuitUpdateInterval)
        {
            UpdatePursuitBehavior();
            lastPursuitUpdate = Time.time;
        }
        
        // Sprite yönünü sürekli kontrol et
        UpdateSpriteDirection();
    }

void OnCollisionEnter2D(Collision2D collision)
    {
        // Player ile çarpışma kontrolü
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControllerNewInput player = collision.gameObject.GetComponent<PlayerControllerNewInput>();
            if (player != null && !player.IsInvincible)
            {
                // Player'a hasar ver
                player.TakeDamage();
            }
            return;
        }
        
        // Kedi-kedi çarpışması kontrolü - daha güçlü kontrol
        Cat otherCat = collision.gameObject.GetComponent<Cat>();
        if (collision.gameObject.CompareTag("Hazard") || otherCat != null)
        {

            
            // Her iki kedinin de physics'ini anında durdur
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.simulated = false; // Physics simülasyonunu tamamen kapat
            }
            
            // Diğer kedinin physics'ini de durdur
            Rigidbody2D otherRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (otherRb != null)
            {
                otherRb.linearVelocity = Vector2.zero;
                otherRb.angularVelocity = 0f;
                otherRb.bodyType = RigidbodyType2D.Kinematic;
                otherRb.simulated = false; // Physics simülasyonunu tamamen kapat
            }
            
            // Collider'ları devre dışı bırak
            Collider2D thisCollider = GetComponent<Collider2D>();
            Collider2D otherCollider = collision.gameObject.GetComponent<Collider2D>();
            if (thisCollider != null) thisCollider.enabled = false;
            if (otherCollider != null) otherCollider.enabled = false;
            
            // İkisini de anında yok et
            Destroy(collision.gameObject);
            Destroy(gameObject);
            return; // Diğer collision kontrollerini atla
        }

        // Duvara çarpma kontrolü - Wall tag ve layer kontrolü
        bool hitWall = collision.gameObject.CompareTag("Ground") || // Wall tag'ı Ground olarak ayarlanmış
                       collision.gameObject.layer == 6 || // Walls layer (layer 6)
                       collision.gameObject.name.ToLower().Contains("wall");

        if (hitWall)
        {
            Vector2 currentVel = rb != null ? rb.linearVelocity : Vector2.zero;
            string wallSide = currentVel.x > 0 ? "SAĞ" : "SOL";
            

            
            // İlk duvara çarpışta bounce kararı ver (sadece bir kez)
            if (!hasDecidedBounce)
            {
                hasDecidedBounce = true;
                canBounce = Random.value < bounceChance;

            }

            // Bounce özelliği varsa ve hala sekme hakkı varsa
            if (canBounce && bounceCount < maxBounces)
            {
                bounceCount++;
                PerformEnhancedBounce(collision);

            }
            else
            {
                // İkinci duvara çarptı veya bounce hakkı yok - ANINDA YOK OL

                
                // Anında yok et (takılma önleme)
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.bodyType = RigidbodyType2D.Kinematic; // Physics'i devre dışı bırak
                }
                Destroy(gameObject); // Anında yok et
            }
        }
    }

    public void SetSpeed(float speed)
    {
        baseSpeed = Mathf.Abs(speed);
        currentSpeed = baseSpeed * speedMultiplier;
        
        // Eğer rb null ise, GetComponent ile al
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        if (rb != null)
        {
            // Spawner'dan gelen hızın işaretini DOĞRU şekilde kullan
            float direction = Mathf.Sign(speed);
            if (direction == 0) direction = 1; // Sıfır durumunda varsayılan sağa
            
            Vector2 newVelocity = new Vector2(direction * currentSpeed, 0f);
            rb.linearVelocity = newVelocity;
            
            // Sprite yönünü güncelle
            UpdateSpriteDirection();
            

        }
        else
        {
            Debug.LogError("CatAI: Rigidbody component not found!");
        }
    }

    /// <summary>
    /// Sets the speed multiplier for difficulty scaling
    /// </summary>
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        currentSpeed = baseSpeed * speedMultiplier;
        
        if (rb != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            float direction = Mathf.Sign(currentVelocity.x);
            rb.linearVelocity = new Vector2(direction * currentSpeed, currentVelocity.y);
        }
    }

    /// <summary>
    /// Bounce özelliğini aktifleştir (10+ puan sonrası)
    /// </summary>
    public void EnableBounce()
    {
        canBounce = true;

    }

    /// <summary>
    /// Enables aim-on-bounce behavior (responds to player Sopa upgrade)
    /// </summary>
    public void EnableAimOnBounce(bool enable)
    {
        aimOnBounce = enable;

    }

    /// <summary>
    /// Enables enhanced pursuit behavior
    /// </summary>
    public void EnablePursuit(bool enable)
    {
        enablePursuit = enable;

    }


    
    /// <summary>
    /// Applies chance-based abilities based on difficulty level and cheese score
    /// Now coordinated with DifficultyManager for better balance
    /// </summary>
    void ApplyChanceBasedAbilities()
    {
        if (GameManager.Instance == null) return;
        
        int currentScore = GameManager.Instance.GetCurrentCheese();
        
        // Get difficulty multiplier from DifficultyManager if available
        float difficultyMultiplier = 1f;
        DifficultyManager difficultyManager = GameManager.Instance.GetDifficultyManager();
        if (difficultyManager != null)
        {
            var difficultyInfo = difficultyManager.GetCurrentDifficultyInfo();
            difficultyMultiplier = 1f + (difficultyInfo.level * 0.1f); // 10% increase per difficulty level
        }
        
        // Base ability chance increases with difficulty
        float baseAbilityChance = 0.25f + (difficultyMultiplier - 1f) * 0.5f; // 25% base, up to 75% at high difficulty
        

        
        // 30 cheese: Bounce ability
        if (currentScore >= 30 && Random.value < baseAbilityChance)
        {
            canBounce = true;
            maxBounces = 1;
            bounceChance = 1f;

        }
        
        // 40 cheese: Aim on bounce
        if (currentScore >= 40 && Random.value < baseAbilityChance)
        {
            aimOnBounce = true;

        }
        
        // 60 cheese: Double bounce
        if (currentScore >= 60 && Random.value < baseAbilityChance)
        {
            canBounce = true;
            maxBounces = 2;
            bounceChance = 1f;

        }
        
        // 70 cheese: Pursuit behavior
        if (currentScore >= 70 && Random.value < baseAbilityChance)
        {
            enablePursuit = true;
            pursuitStrength = 0.4f * difficultyMultiplier;
            pursuitUpdateInterval = Mathf.Max(0.1f, 0.3f / difficultyMultiplier);

        }
        
        // 80 cheese: Speed boost
        if (currentScore >= 80 && Random.value < baseAbilityChance)
        {
            speedMultiplier = 1.5f + (difficultyMultiplier - 1f) * 0.5f; // Scales with difficulty
            currentSpeed = baseSpeed * speedMultiplier;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, 0.4f);
            }
            

        }
        
        // 100 cheese: Super cat (multiple abilities)
        if (currentScore >= 100 && Random.value < baseAbilityChance * 0.8f) // Slightly lower chance for super cats
        {
            canBounce = true;
            maxBounces = Mathf.RoundToInt(2 + difficultyMultiplier);
            bounceChance = 1f;
            aimOnBounce = true;
            enablePursuit = true;
            pursuitStrength = 0.6f * difficultyMultiplier;
            speedMultiplier = 1.3f + (difficultyMultiplier - 1f) * 0.3f;
            currentSpeed = baseSpeed * speedMultiplier;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, 0.5f);
            }
            

        }
        
        // 120 cheese: Elite tracking
        if (currentScore >= 120 && Random.value < baseAbilityChance)
        {
            enablePursuit = true;
            pursuitStrength = 0.7f * difficultyMultiplier;
            pursuitUpdateInterval = Mathf.Max(0.05f, 0.1f / difficultyMultiplier);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.magenta, 0.6f);
            }
            

        }
    }

    /// <summary>
    /// Performs enhanced bounce with reliable wall repulsion and player aiming
    /// </summary>
void PerformEnhancedBounce(Collision2D collision = null)
    {
        if (rb == null) return;
        
        Vector2 currentVelocity = rb.linearVelocity;
        
        // Collision normal'ından yön belirle (daha güvenilir)
        Vector2 newVelocity;
        string wallSide = "UNKNOWN";
        
        if (collision != null && collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            
            // Normal vektörüne göre yön belirle
            if (normal.x > 0.5f) // Sol duvar (normal sağa işaret ediyor)
            {
                newVelocity = new Vector2(currentSpeed, 0f); // Sağa git
                wallSide = "SOL";
            }
            else if (normal.x < -0.5f) // Sağ duvar (normal sola işaret ediyor)
            {
                newVelocity = new Vector2(-currentSpeed, 0f); // Sola git
                wallSide = "SAĞ";
            }
            else
            {
                // Fallback: mevcut hızın tersini al
                newVelocity = new Vector2(-Mathf.Sign(currentVelocity.x) * currentSpeed, 0f);
                wallSide = currentVelocity.x > 0 ? "SAĞ" : "SOL";
            }
        }
        else
        {
            // Collision bilgisi yoksa, basit ters yön
            newVelocity = new Vector2(-Mathf.Sign(currentVelocity.x) * currentSpeed, 0f);
            wallSide = currentVelocity.x > 0 ? "SAĞ" : "SOL";
        }
        
        // Eğer hala yön belirlenemiyorsa, rastgele seç
        if (Mathf.Abs(newVelocity.x) < 0.1f)
        {
            newVelocity.x = Random.value > 0.5f ? currentSpeed : -currentSpeed;
            wallSide = "RASTGELE";
        }
        
        // PLAYER AIMING: If aimOnBounce is enabled, adjust trajectory toward player
        if (aimOnBounce && playerTransform != null)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            
            // Blend the bounce direction with player direction
            float aimStrength = 0.6f; // How much to aim toward player (60%)
            Vector2 aimDirection = Vector2.Lerp(newVelocity.normalized, directionToPlayer, aimStrength);
            
            // Maintain speed but adjust direction
            newVelocity = aimDirection.normalized * currentSpeed;
            

        }
        
        string newDirection = newVelocity.x > 0 ? "SAĞ" : "SOL";

        
        // Mevcut hareketi tamamen durdur
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Duvardan uzaklaştır
        if (collision != null && collision.contacts.Length > 0)
        {
            Vector2 separation = collision.contacts[0].normal * 0.3f;
            transform.position = (Vector2)transform.position + separation;
        }
        
        // Yeni hızı anında uygula
        rb.linearVelocity = newVelocity;
        
        // Sprite yönünü güncelle
        UpdateSpriteDirection();
        

    }
    


    /// <summary>
    /// Updates pursuit behavior to gradually adjust trajectory towards player
    /// </summary>
    void UpdatePursuitBehavior()
    {
        if (playerTransform == null || rb == null) return;

        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        
        // Calculate desired velocity towards player
        Vector2 desiredVelocity = directionToPlayer * currentSpeed;
        
        // Blend current velocity with desired velocity
        Vector2 newVelocity = Vector2.Lerp(currentVelocity, desiredVelocity, pursuitStrength * Time.deltaTime);
        
        // Maintain speed consistency
        newVelocity = newVelocity.normalized * currentSpeed;
        
        rb.linearVelocity = newVelocity;
        

    }

    /// <summary>
    /// Gets current cat AI state for debugging
    /// </summary>
    public string GetDebugInfo()
    {
        return $"Speed: {currentSpeed:F1}, Bounces: {bounceCount}/{maxBounces}, AimOnBounce: {aimOnBounce}, Pursuit: {enablePursuit}";
    }

    /// <summary>
    /// Makes the cat more aggressive (visual/audio feedback)
    /// </summary>
    void BecomeAggressive()
    {
        // Play aggressive cat sound
        AudioManager.Instance?.PlayCatMeow();
        
        // TODO: Add visual effects (red tint, larger size, etc.)
        

    }

    /// <summary>
    /// Resets cat to normal behavior
    /// </summary>
    public void ResetBehavior()
    {
        aimOnBounce = false;
        enablePursuit = false;
        speedMultiplier = 1f;
        currentSpeed = baseSpeed;
        pursuitStrength = 0.3f;
        
        if (rb != null)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            float direction = Mathf.Sign(currentVelocity.x);
            rb.linearVelocity = new Vector2(direction * currentSpeed, currentVelocity.y);
        }
        

    }

    /// <summary>
    /// Gets the threat level of this cat (for UI/feedback purposes)
    /// </summary>
    public int GetThreatLevel()
    {
        int threatLevel = 1; // Base threat
        
        if (canBounce) threatLevel++;
        if (aimOnBounce) threatLevel++;
        if (enablePursuit) threatLevel++;
        if (speedMultiplier > 1.2f) threatLevel++;
        
        return Mathf.Clamp(threatLevel, 1, 5);
    }

    /// <summary>
    /// Updates sprite direction based on movement direction
    /// </summary>
    void UpdateSpriteDirection()
    {
        // Component'leri kontrol et ve gerekirse al
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        if (spriteRenderer == null || rb == null) return;

        Vector2 velocity = rb.linearVelocity;
        
        // Hız çok küçükse yön değiştirme
        if (Mathf.Abs(velocity.x) < 0.1f) return;
        
        bool shouldFaceRight = velocity.x > 0;

        // Sadece yön değiştiğinde sprite'ı çevir
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            spriteRenderer.flipX = !facingRight; // flipX true olduğunda sola bakar
            

        }
    }
    

}
