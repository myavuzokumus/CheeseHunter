using UnityEngine;

/// <summary>
/// Hazard spawn olmadan önce 1 saniye warning gösterir (Fade efektli)
/// </summary>
public class HazardWarningIndicator : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private float fadeDuration = 1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("HazardWarningIndicator: SpriteRenderer bulunamadı!");
            return;
        }

        // Orijinal rengi kaydet
        originalColor = spriteRenderer.color;

        // Başlangıçta şeffaf
        Color startColor = originalColor;
        startColor.a = 0f;
        spriteRenderer.color = startColor;
    }

    void Update()
    {
        if (spriteRenderer == null) return;

        timer += Time.deltaTime;
        float progress = timer / fadeDuration;

        // Fade in -> Fade out efekti (smooth)
        float alpha;
        if (progress < 0.5f)
        {
            // İlk yarı: Fade In (0 -> 1)
            alpha = Mathf.Lerp(0f, 1f, progress * 2f);
        }
        else
        {
            // İkinci yarı: Fade Out (1 -> 0)
            alpha = Mathf.Lerp(1f, 0f, (progress - 0.5f) * 2f);
        }

        Color currentColor = originalColor;
        currentColor.a = alpha;
        spriteRenderer.color = currentColor;

        // Hafif pulse efekti
        float scale = 1f + Mathf.Sin(timer * 8f) * 0.1f;
        transform.localScale = Vector3.one * scale;
    }
}
