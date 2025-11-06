using UnityEngine;

/// <summary>
/// Automatically destroys the GameObject after a specified lifetime
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Header("Auto Destroy Settings")]
    public float lifetime = 2f;
    
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}