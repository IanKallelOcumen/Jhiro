using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackButtonHook : MonoBehaviour
{
    [Tooltip("Optional: drag your MainMenuController here. If left empty, it will auto-find at runtime.")]
    public MainMenuController controller;

    Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
    }

    void OnEnable()
    {
        if (controller == null)
        {
#if UNITY_2023_1_OR_NEWER
            // Newer API uses enum instead of bool
            controller = Object.FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
#else
            // Works in older Unity versions and finds inactive objects too
            var all = Resources.FindObjectsOfTypeAll<MainMenuController>();
            controller = (all != null && all.Length > 0) ? all[0] : null;
#endif
        }

        if (_btn != null && controller != null)
        {
            _btn.onClick.RemoveListener(controller.OnBack);
            _btn.onClick.AddListener(controller.OnBack);
#if UNITY_EDITOR
            Debug.Log("[BackButtonHook] Bound OnBack to " + gameObject.name);
#endif
        }
    }

    void OnDisable()
    {
        if (_btn != null && controller != null)
            _btn.onClick.RemoveListener(controller.OnBack);
    }
}
