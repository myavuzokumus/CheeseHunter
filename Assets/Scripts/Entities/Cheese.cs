using UnityEngine;

/// <summary>
/// Simple collectible item (cheese) that gives points when collected
/// </summary>
public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int pointValue = 1;
    [SerializeField] private bool destroyOnCollect = true;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Give points to player
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCheese(pointValue);
                Debug.Log($"Collectible: Player collected cheese! Points: {pointValue}");
            }
            
            // Play collect effect
            if (VFXManager.Instance != null)
            {
                Debug.Log($"Collectible: Calling PlayCheeseScoreEffect at position {transform.position}");
                VFXManager.Instance.PlayCheeseScoreEffect(transform.position);
            }
            else
            {
                Debug.LogWarning("Collectible: VFXManager.Instance is null!");
            }
            
            // Play collect sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCheeseCollection();
            }
            
            // Destroy collectible
            if (destroyOnCollect)
            {
                Destroy(gameObject);
            }
        }
    }
}