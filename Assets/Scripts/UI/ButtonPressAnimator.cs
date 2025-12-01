using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any Button to give it a "squish" animation on press.
/// Fulfills the "animations for pressing" request.
/// </summary>
public class ButtonPressAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("How small the button gets on press (e.g., 0.9)")]
    public float scaleOnPress = 0.9f;

    private Vector3 _originalScale;

    void Awake()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Scale down when pressed
        transform.localScale = _originalScale * scaleOnPress;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Return to original scale when released
        transform.localScale = _originalScale;
    }

    // Also return to original scale if pointer leaves
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = _originalScale;
    }
}