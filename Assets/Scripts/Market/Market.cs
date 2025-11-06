using UnityEngine;

/// <summary>
/// Handles market interaction when player touches it
/// </summary>
public class MarketTrigger : MonoBehaviour
{
    void Start()
    {

        
        // Check collider setup
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {

        }
        else
        {
            Debug.LogError("MarketTrigger: No collider found!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {

        
        if (other.CompareTag("Player"))
        {

            OpenMarket();
        }
        else
        {

        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }
    
    void OpenMarket()
    {

        
        // Open market UI first
        MarketUI marketUI = FindFirstObjectByType<MarketUI>();
        if (marketUI != null)
        {

            marketUI.OpenMarket();
        }
        else
        {
            Debug.LogError("MarketTrigger: MarketUI not found in scene!");
            
            // Try to create MarketUI if it doesn't exist
            GameObject marketUIObj = new GameObject("MarketUI");
            MarketUI newMarketUI = marketUIObj.AddComponent<MarketUI>();
            
            // Wait a frame for it to initialize, then open
            StartCoroutine(OpenMarketAfterDelay(newMarketUI));
        }
        
        // Destroy this market
        Destroy(gameObject);
    }
    
    System.Collections.IEnumerator OpenMarketAfterDelay(MarketUI marketUI)
    {
        yield return null; // Wait one frame
        if (marketUI != null)
        {
            marketUI.OpenMarket();
        }
    }
}