using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement; // <--- REQUIRED FOR LEVEL LOADING

/// <summary>
/// Attach this to each World Prefab.
/// Features: Floating, Rainbow Outline, Highlight, Bump Effects, and Scene Loading.
/// </summary>
public class WorldItem : MonoBehaviour
{
    [Header("Data")]
    public string worldName = "Ice World";
    public string sceneToLoad = "IceLevel1";
    public string progress = "0/15";

    [Header("Locking")]
    public bool startUnlocked = false;
    public string worldSaveID;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    
    [Header("Visual References")]
    public SpriteRenderer worldSpriteRenderer; 
    public SpriteRenderer outlineSprite;       
    public GameObject worldCanvas;
    
    public TextMeshProUGUI worldNameText;
    public TextMeshProUGUI progressText;
    public Button playButton;

    [Header("Selection Highlight")]
    public Color selectedColor = Color.white;        
    public Color unselectedColor = new Color(0.6f, 0.6f, 0.6f, 1f); 

    [Header("Rainbow Outline FX")]
    public float outlineHueSpeed = 0.5f;
    [Range(0f, 1f)] public float outlineSaturation = 1f;
    [Range(0f, 1f)] public float outlineValue = 1f;

    [Header("Floating FX")]
    public float floatAmplitude = 0.03f;
    public float floatSpeed = 0.8f;
    public float floatRotAmplitude = 0.5f;
    public float floatRotSpeed = 0.5f;

    [Header("Animations")]
    public Vector3 selectedScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float scaleDuration = 0.2f;
    public float shakeDuration = 1.5f;
    public float maxShakeAmount = 0.2f; 
    public float popDuration = 0.3f;
    
    // Internal State
    private Vector3 _deselectedScale = Vector3.one;
    private bool _isLocked = true;
    private bool _isSelected = false;
    private Vector3 _basePos;
    private float _randomPhase;

    void Awake()
    {
        if (worldSpriteRenderer == null)
            worldSpriteRenderer = GetComponent<SpriteRenderer>();
        
        _basePos = transform.localPosition; 
        _randomPhase = Random.value * Mathf.PI * 2f; 

        // --- SCENE LOAD FIX: Setup Button ---
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(LoadWorldScene);
        }

        if (startUnlocked)
        {
            _isLocked = false;
            // Ensure this static method exists in your project, otherwise comment it out
            GameProgressManager.SaveBookUnlockState(worldSaveID, true);
        }
        else
        {
            // Ensure this static method exists in your project
            _isLocked = !GameProgressManager.IsBookUnlocked(worldSaveID);
        }
    }

    void OnEnable()
    {
        bool isNowUnlocked = GameProgressManager.IsBookUnlocked(worldSaveID);
        if (_isLocked && isNowUnlocked) 
        {
            StartCoroutine(UnlockRoutine());
        }
        else
        {
            UpdateVisuals();
        }
    }

    void Update()
    {
        // 1. Floating (Applied to local position relative to slider parent)
        float t = Time.time;
        float yOffset = Mathf.Sin(t * floatSpeed + _randomPhase) * floatAmplitude;
        float zRot = Mathf.Sin(t * floatRotSpeed + _randomPhase * 0.8f) * floatRotAmplitude;

        transform.localPosition = new Vector3(_basePos.x, _basePos.y + yOffset, _basePos.z);
        transform.localEulerAngles = new Vector3(0, 0, zRot);

        // 2. Rainbow Outline
        if (_isSelected && !_isLocked && outlineSprite != null)
        {
            float hue = (Time.unscaledTime * outlineHueSpeed) % 1f; 
            Color rainbow = Color.HSVToRGB(hue, outlineSaturation, outlineValue);
            outlineSprite.enabled = true;
            outlineSprite.color = rainbow;
        }
        else if (outlineSprite != null)
        {
            outlineSprite.enabled = false;
        }
    }

    public void Initialize(bool isSelected)
    {
        _isSelected = isSelected;
        UpdateVisuals();
        transform.localScale = isSelected ? selectedScale : _deselectedScale;
    }

    public void Select()
    {
        _isSelected = true;
        UpdateVisuals();
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(selectedScale));
    }

    public void Deselect()
    {
        _isSelected = false;
        UpdateVisuals();
        StopAllCoroutines();
        StartCoroutine(ScaleRoutine(_deselectedScale));
    }

    public void Bump()
    {
        if (_isSelected && !_isLocked)
        {
            StopAllCoroutines();
            StartCoroutine(BumpRoutine());
        }
    }

    // --- SCENE LOADING FUNCTION ---
    public void LoadWorldScene()
    {
        if (_isLocked) return;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Optional: Play a sound here
            Debug.Log($"Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError($"Scene name is empty on WorldItem: {worldName}");
        }
    }

    public void UpdateVisuals()
    {
        if (worldNameText) worldNameText.text = worldName;

        if (_isLocked)
        {
            worldSpriteRenderer.color = lockedColor;
            if (outlineSprite) outlineSprite.enabled = false;
            if (worldCanvas) worldCanvas.SetActive(_isSelected);
            if (playButton) playButton.gameObject.SetActive(false);
            if (progressText) progressText.text = "LOCKED";
        }
        else
        {
            worldSpriteRenderer.color = _isSelected ? selectedColor : unselectedColor;
            if (worldCanvas) worldCanvas.SetActive(_isSelected);
            if (playButton) playButton.gameObject.SetActive(true);
            if (progressText) progressText.text = progress;
        }
    }

    // --- COROUTINES ---

    IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < scaleDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / scaleDuration;
            // Using standard Lerp if Easing class is missing, otherwise use Easing.CubicEaseOut(p)
            transform.localScale = Vector3.Lerp(startScale, targetScale, p); 
            yield return null;
        }
        transform.localScale = targetScale;
    }

    IEnumerator BumpRoutine()
    {
        // Quick punch up to 1.15x the selected scale
        Vector3 punchScale = selectedScale * 1.15f;
        float duration = 0.15f;
        float t = 0;

        // Up
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            // Assuming Easing class exists based on your snippet
            transform.localScale = Vector3.Lerp(selectedScale, punchScale, Easing.CubicEaseOut(p));
            yield return null;
        }

        // Down
        t = 0;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / duration;
            transform.localScale = Vector3.Lerp(punchScale, selectedScale, Easing.CubicEaseIn(p));
            yield return null;
        }
        transform.localScale = selectedScale;
    }

    IEnumerator UnlockRoutine()
    {
        _isLocked = false; 
        float t = 0f;
        Vector3 shakeBasePos = transform.localPosition;
        
        // 1. Shake Phase
        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = t / shakeDuration; 
            float easeP = Easing.CubicEaseIn(p); 
            float shakeAmount = easeP * maxShakeAmount;
            
            transform.localPosition = shakeBasePos + (Vector3)Random.insideUnitCircle * shakeAmount;
            worldSpriteRenderer.color = Color.Lerp(lockedColor, unselectedColor, easeP);
            yield return null;
        }

        transform.localPosition = shakeBasePos;
        UpdateVisuals(); 

        // 2. Pop Up Phase (Scale Up)
        float tPop = 0;
        float startScaleVal = _deselectedScale.x;
        float endScaleVal = selectedScale.x * 1.3f; // Pop slightly larger than max

        while (tPop < popDuration / 2f)
        {
            tPop += Time.unscaledDeltaTime;
            float p = tPop / (popDuration / 2f);
            float s = Mathf.Lerp(startScaleVal, endScaleVal, Easing.CubicEaseOut(p));
            transform.localScale = Vector3.one * s;
            yield return null;
        }

        // 3. Pop Down Phase (Settle)
        tPop = 0;
        while (tPop < popDuration / 2f)
        {
            tPop += Time.unscaledDeltaTime;
            float p = tPop / (popDuration / 2f);
            float s = Mathf.Lerp(endScaleVal, selectedScale.x, Easing.CubicEaseIn(p));
            transform.localScale = Vector3.one * s;
            yield return null;
        }

        transform.localScale = selectedScale;
        
        // Auto-select after unlock if you want
        Select(); 
    }
}