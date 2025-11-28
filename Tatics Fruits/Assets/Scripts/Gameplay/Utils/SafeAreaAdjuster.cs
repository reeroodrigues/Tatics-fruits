using System;
using UnityEngine;

public class SafeAreaAdjuster : MonoBehaviour
{
    [Tooltip("Aplique somente no eixo X/Y")]
    public bool applyX = true;
    public bool applyY = true;
    
    [Tooltip("Margens suaves extras (px): Left, Top, Right, Bottom")]
    public Vector4 softMargins = Vector4.zero;

    private Rect _lastSafe;
    private Vector2 _lastScreen;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        Apply();
    }

    private void Start()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }
    
#if UNITY_EDITOR
    private void Update()
    {
        ApplyIfChanged();
    }
#endif

    private void OnRectTransformDimensionsChange()
    {
        ApplyIfChanged();
    }

    private void ApplyIfChanged()
    {
        if (_lastSafe != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
        {
            Apply();
        }
    }

    public void Apply()
    {
        if(_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
        
        var safe = Screen.safeArea;
        _lastSafe = safe;
        _lastScreen = new Vector2(Screen.width, Screen.height);
        
        var min = safe.position;
        var max = safe.position + safe.size;
        min.x /= Screen.width;
        max.x /= Screen.width;
        min.y /= Screen.height;
        max.y /= Screen.height;

        if (!applyX)
        {
            min.x = 0f;
            max.x = 1f;
        }

        if (!applyY)
        {
            min.y = 0f;
            max.y = 1f;
        }
        
        _rectTransform.anchorMin = min;
        _rectTransform.anchorMax = max;
        
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;

        if (softMargins != Vector4.zero)
        {
            float w = Screen.width, h = Screen.height;
            var m = _rectTransform.anchorMin;
            var M = _rectTransform.anchorMax;
            m.x += softMargins.x / w;
            M.x -= softMargins.z / w;
            M.y -= softMargins.y / h;
            m.y += softMargins.w / h;
            _rectTransform.anchorMin = m;
            _rectTransform.anchorMax = M;
        }
    }
}
