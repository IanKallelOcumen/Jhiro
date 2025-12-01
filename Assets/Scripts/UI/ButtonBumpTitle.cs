using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonBumpUI : MonoBehaviour, IPointerDownHandler
{
    [Tooltip("The UI element to trigger a bump on (e.g., the Title or the Book itself).")]
    public UIFloat target;
    [Tooltip("The background element to trigger a kick on.")]
    public BackgroundWiggle bg;

    void Awake()
    {
        if (target == null)
        {
            // Auto-find the UI element this button belongs to, or the Main Title if not found.
            target = GetComponentInParent<UIFloat>();
            if (target == null)
            {
#if UNITY_2023_1_OR_NEWER
                target = Object.FindFirstObjectByType<UIFloat>();
#else
                target = FindObjectOfType<UIFloat>();
#endif
            }
        }
        
        if (bg == null)
        {
#if UNITY_2023_1_OR_NEWER
            bg = Object.FindFirstObjectByType<BackgroundWiggle>();
#else
            bg = FindObjectOfType<BackgroundWiggle>();
#endif
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (target != null) target.TriggerBump();
        if (bg != null)     bg.Kick(1f);
    }
}
