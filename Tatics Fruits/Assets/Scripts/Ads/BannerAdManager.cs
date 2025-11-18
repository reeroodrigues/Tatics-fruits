using UnityEngine;

namespace Ads
{
    public class BannerAdManager : MonoBehaviour
    {
        [Header("Banner Settings")]
        [SerializeField] private bool showBannerOnStart = true;
        [SerializeField] private bool enableBanners = true;

        private IBannerAdProvider _bannerProvider;

        public static BannerAdManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (enableBanners && showBannerOnStart && _bannerProvider != null)
            {
                ShowBanner();
            }
        }

        public void SetBannerProvider(IBannerAdProvider provider)
        {
            if (_bannerProvider != null)
            {
                _bannerProvider.DestroyBanner();
            }

            _bannerProvider = provider;
            Debug.Log($"[BannerAdManager] Banner provider set: {provider.GetType().Name}");

            if (enableBanners && showBannerOnStart)
            {
                ShowBanner();
            }
        }

        public void ShowBanner()
        {
            if (!enableBanners)
            {
                Debug.Log("[BannerAdManager] Banners are disabled");
                return;
            }

            if (_bannerProvider == null)
            {
                Debug.LogWarning("[BannerAdManager] No banner provider set!");
                return;
            }

            Debug.Log("[BannerAdManager] Showing banner ad");
            _bannerProvider.LoadAndShowBanner();
        }

        public void HideBanner()
        {
            if (_bannerProvider == null)
            {
                Debug.LogWarning("[BannerAdManager] No banner provider set!");
                return;
            }

            Debug.Log("[BannerAdManager] Hiding banner ad");
            _bannerProvider.HideBanner();
        }

        public void DestroyBanner()
        {
            if (_bannerProvider == null) return;

            Debug.Log("[BannerAdManager] Destroying banner ad");
            _bannerProvider.DestroyBanner();
        }

        public bool IsBannerShowing()
        {
            return _bannerProvider != null && _bannerProvider.IsBannerShowing();
        }

        public void EnableBanners(bool enable)
        {
            enableBanners = enable;
            
            if (!enable && _bannerProvider != null)
            {
                HideBanner();
            }
            else if (enable && _bannerProvider != null)
            {
                ShowBanner();
            }
        }

        private void OnDestroy()
        {
            if (_bannerProvider != null)
            {
                _bannerProvider.DestroyBanner();
            }
        }
    }
}
