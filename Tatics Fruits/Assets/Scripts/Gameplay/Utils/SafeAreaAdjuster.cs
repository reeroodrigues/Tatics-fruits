using UnityEngine;

namespace Gameplay.Utils
{
    public class SafeAreaAdjuster : MonoBehaviour
    {
        [Tooltip("Apply only on X/Y axis")]
        public bool applyX = true;
        public bool applyY = true;
        
        [Tooltip("Extra soft margins (px): Left, Top, Right, Bottom")]
        public Vector4 softMargins = Vector4.zero;

        private Rect lastSafe;
        private Vector2 lastScreen;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
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
            if (lastSafe != Screen.safeArea || lastScreen.x != Screen.width || lastScreen.y != Screen.height)
            {
                Apply();
            }
        }

        public void Apply()
        {
            if(rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            var safe = Screen.safeArea;
            lastSafe = safe;
            lastScreen = new Vector2(Screen.width, Screen.height);
            
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
            
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
            
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            if (softMargins != Vector4.zero)
            {
                float w = Screen.width, h = Screen.height;
                var m = rectTransform.anchorMin;
                var M = rectTransform.anchorMax;
                m.x += softMargins.x / w;
                M.x -= softMargins.z / w;
                M.y -= softMargins.y / h;
                m.y += softMargins.w / h;
                rectTransform.anchorMin = m;
                rectTransform.anchorMax = M;
            }
            
            Debug.Log($"[SafeArea] Applied safe area: {safe}, Screen: {Screen.width}x{Screen.height}");
        }
    }
}
