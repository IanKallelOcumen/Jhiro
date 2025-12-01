using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to the "Play" Button inside your World Prefab.
/// It triggers the World Bump and the Background Kick when clicked.
/// </summary>
public class WorldButtonFeedback : MonoBehaviour, IPointerDownHandler
{
    [Header("References")]
    [Tooltip("The WorldItem this button belongs to. Leave empty to auto-find.")]
    public WorldItem worldItem;
    
    [Tooltip("The BackgroundWiggle script in the scene. Leave empty to auto-find.")]
    public BackgroundWiggle backgroundWiggle;

    [Header("Settings")]
    public float kickAmount = 6f; // How hard to kick the background

    void Start()
    {
        // Auto-find WorldItem if not assigned (looks in parents)
        if (worldItem == null)
        {
            worldItem = GetComponentInParent<WorldItem>();
        }

        // Auto-find BackgroundWiggle (looks in scene)
        if (backgroundWiggle == null)
        {
#if UNITY_2023_1_OR_NEWER
            backgroundWiggle = Object.FindFirstObjectByType<BackgroundWiggle>();
#else
            backgroundWiggle = FindObjectOfType<BackgroundWiggle>();
#endif
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Bump the World Visual
        if (worldItem != null)
        {
            worldItem.Bump();
        }

        // 2. Kick the Background
        if (backgroundWiggle != null)
        {
            backgroundWiggle.Kick(kickAmount);
        }
    }
}