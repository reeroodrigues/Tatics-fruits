using UnityEngine;
using UnityEngine.UI;

namespace Ads
{
    public class TestBannerVisualizer : MonoBehaviour
    {
        [Header("Visualizer Settings")]
        [SerializeField] private bool showInEditor = true;
        [SerializeField] private Color bannerColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        [SerializeField] private string bannerText = "📱 BANNER AD (320x50)";
        
        private GameObject _visualBanner;
        private Canvas _canvas;

        private void Start()
        {
            if (Application.isEditor && showInEditor)
            {
                CreateVisualBanner();
            }
        }

        private void CreateVisualBanner()
        {
            _canvas = FindObjectOfType<Canvas>();
            
            if (_canvas == null)
            {
                Debug.LogWarning("[TestBannerVisualizer] No Canvas found in scene. Cannot create visual banner.");
                return;
            }

            _visualBanner = new GameObject("VISUAL_BANNER_AD");
            _visualBanner.transform.SetParent(_canvas.transform, false);

            var rectTransform = _visualBanner.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(320f, 50f);

            var image = _visualBanner.AddComponent<Image>();
            image.color = bannerColor;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(_visualBanner.transform, false);
            
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.AddComponent<Text>();
            text.text = bannerText;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            _visualBanner.transform.SetAsLastSibling();

            Debug.Log("[TestBannerVisualizer] ✅ Visual banner created at bottom of screen (Editor only)");
        }

        public void Show()
        {
            if (_visualBanner != null)
            {
                _visualBanner.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_visualBanner != null)
            {
                _visualBanner.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_visualBanner != null)
            {
                Destroy(_visualBanner);
            }
        }
    }
}
