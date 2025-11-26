using GoogleMobileAds.Api;
using UnityEngine;

namespace Ads
{
    public class AdMobBannerProvider : IBannerAdProvider
    {
        private const string ANDROID_AD_UNIT_ID = "ca-app-pub-8609532543875876/5682573689";
        private const string IOS_AD_UNIT_ID = "ca-app-pub-8609532543875876/5682573689";

        private string _adUnitId;
        private BannerView _bannerView;
        private bool _isBannerShowing;

        public AdMobBannerProvider()
        {
#if UNITY_ANDROID
            _adUnitId = ANDROID_AD_UNIT_ID;
#elif UNITY_IOS
            _adUnitId = IOS_AD_UNIT_ID;
#else
            _adUnitId = ANDROID_AD_UNIT_ID;
#endif

            Debug.Log($"[AdMobBannerProvider] Initialized with ad unit ID: {_adUnitId}");
        }

        public void LoadAndShowBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("[AdMobBannerProvider] Banner already exists. Showing existing banner.");
                _bannerView.Show();
                _isBannerShowing = true;
                return;
            }

            Debug.Log($"[AdMobBannerProvider] Creating and loading banner ad with ID: {_adUnitId}");

            _bannerView = new BannerView(_adUnitId, AdSize.Banner, AdPosition.Bottom);

            RegisterEventHandlers();

            AdRequest adRequest = new AdRequest();
            _bannerView.LoadAd(adRequest);
        }

        public void HideBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("[AdMobBannerProvider] Hiding banner ad");
                _bannerView.Hide();
                _isBannerShowing = false;
            }
        }

        public void DestroyBanner()
        {
            if (_bannerView != null)
            {
                Debug.Log("[AdMobBannerProvider] Destroying banner ad");
                _bannerView.Destroy();
                _bannerView = null;
                _isBannerShowing = false;
            }
        }

        public bool IsBannerShowing()
        {
            return _isBannerShowing;
        }

        private void RegisterEventHandlers()
        {
            _bannerView.OnBannerAdLoaded += () =>
            {
                Debug.Log("[AdMobBannerProvider] ✅ Banner ad loaded successfully!");
                _isBannerShowing = true;
            };

            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                Debug.LogError($"[AdMobBannerProvider] ❌ Banner ad failed to load");
                Debug.LogError($"[AdMobBannerProvider] Error Code: {error.GetCode()}");
                Debug.LogError($"[AdMobBannerProvider] Error Message: {error.GetMessage()}");
                _isBannerShowing = false;
            };

            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"[AdMobBannerProvider] Banner ad paid {adValue.Value} {adValue.CurrencyCode}");
            };

            _bannerView.OnAdImpressionRecorded += () =>
            {
                Debug.Log("[AdMobBannerProvider] Banner ad impression recorded");
            };

            _bannerView.OnAdClicked += () =>
            {
                Debug.Log("[AdMobBannerProvider] Banner ad clicked");
            };

            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[AdMobBannerProvider] Banner ad full screen content opened");
            };

            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdMobBannerProvider] Banner ad full screen content closed");
            };
        }
    }
}
