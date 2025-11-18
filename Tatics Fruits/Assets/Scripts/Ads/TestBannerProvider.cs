using UnityEngine;
using UnityEngine.UI;

namespace Ads
{
    public class TestBannerProvider : MonoBehaviour, IBannerAdProvider
    {
        private GameObject _bannerObject;
        private bool _isBannerShowing;

        public void LoadAndShowBanner()
        {
            if (_bannerObject != null)
            {
                Debug.Log("[TestBannerProvider] Banner already exists. Showing it.");
                _bannerObject.SetActive(true);
                _isBannerShowing = true;
                return;
            }

            Debug.Log("[TestBannerProvider] Creating test banner ad");
            CreateTestBanner();
            _isBannerShowing = true;
        }

        public void HideBanner()
        {
            if (_bannerObject != null)
            {
                Debug.Log("[TestBannerProvider] Hiding test banner");
                _bannerObject.SetActive(false);
                _isBannerShowing = false;
            }
        }

        public void DestroyBanner()
        {
            if (_bannerObject != null)
            {
                Debug.Log("[TestBannerProvider] Destroying test banner");
                Destroy(_bannerObject);
                _bannerObject = null;
                _isBannerShowing = false;
            }
        }

        public bool IsBannerShowing()
        {
            return _isBannerShowing && _bannerObject != null && _bannerObject.activeSelf;
        }

        private void CreateTestBanner()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TestBannerProvider] No Canvas found in scene!");
                return;
            }

            _bannerObject = new GameObject("TestBannerAd");
            _bannerObject.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = _bannerObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0, 50);

            Image background = _bannerObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            GameObject textObj = new GameObject("BannerText");
            textObj.transform.SetParent(_bannerObject.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.text = "🎮 TEST BANNER AD 🎮";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.yellow;

            Debug.Log("[TestBannerProvider] ✅ Test banner created at bottom of screen");
        }

        private void OnDestroy()
        {
            DestroyBanner();
        }
    }
}
