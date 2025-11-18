using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Ads
{
    public class AdMobInterstitialProvider : IInterstitialAdProvider
    {
        private const string ANDROID_AD_UNIT_ID = "ca-app-pub-3940256099942544/1033173712";
        private const string IOS_AD_UNIT_ID = "ca-app-pub-3940256099942544/4411468910";

        private string _adUnitId;
        private InterstitialAd _interstitialAd;
        private Action _onAdCompleted;
        private Action _onAdFailed;

        public AdMobInterstitialProvider()
        {
#if UNITY_ANDROID
            _adUnitId = ANDROID_AD_UNIT_ID;
#elif UNITY_IOS
            _adUnitId = IOS_AD_UNIT_ID;
#else
            _adUnitId = ANDROID_AD_UNIT_ID;
#endif

            Debug.Log($"[AdMobInterstitialProvider] Initialized with ad unit ID: {_adUnitId}");
        }

        public bool IsAdReady()
        {
            return _interstitialAd != null && _interstitialAd.CanShowAd();
        }

        public void ShowAd(Action onAdCompleted, Action onAdFailed)
        {
            _onAdCompleted = onAdCompleted;
            _onAdFailed = onAdFailed;

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                Debug.Log($"[AdMobInterstitialProvider] Showing interstitial ad with ID: {_adUnitId}");
                _interstitialAd.Show();
            }
            else
            {
                Debug.LogError("[AdMobInterstitialProvider] Interstitial ad is not ready to be shown.");
                _onAdFailed?.Invoke();
            }
        }

        public void LoadAd()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            Debug.Log($"[AdMobInterstitialProvider] Loading interstitial ad with ID: {_adUnitId}");

            AdRequest adRequest = new AdRequest();

            InterstitialAd.Load(_adUnitId, adRequest, OnAdLoadCallback);
        }

        private void OnAdLoadCallback(InterstitialAd ad, LoadAdError error)
        {
            if (error != null || ad == null)
            {
                Debug.LogError($"[AdMobInterstitialProvider] ❌ FAILED TO LOAD AD");
                Debug.LogError($"[AdMobInterstitialProvider] Error Code: {error?.GetCode()}");
                Debug.LogError($"[AdMobInterstitialProvider] Error Domain: {error?.GetDomain()}");
                Debug.LogError($"[AdMobInterstitialProvider] Error Message: {error?.GetMessage()}");
                Debug.LogError($"[AdMobInterstitialProvider] Error Cause: {error?.GetCause()}");
                Debug.LogError($"[AdMobInterstitialProvider] Response Info: {error?.GetResponseInfo()}");
                return;
            }

            Debug.Log($"[AdMobInterstitialProvider] ✅ AD LOADED SUCCESSFULLY!");
            Debug.Log($"[AdMobInterstitialProvider] Response: {ad.GetResponseInfo()}");
            _interstitialAd = ad;
            RegisterEventHandlers(ad);
        }

        private void RegisterEventHandlers(InterstitialAd ad)
        {
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log($"[AdMobInterstitialProvider] Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
            };

            ad.OnAdImpressionRecorded += () =>
            {
                Debug.Log("[AdMobInterstitialProvider] Interstitial ad recorded an impression.");
            };

            ad.OnAdClicked += () =>
            {
                Debug.Log("[AdMobInterstitialProvider] Interstitial ad was clicked.");
            };

            ad.OnAdFullScreenContentOpened += () =>
            {
                Debug.Log("[AdMobInterstitialProvider] Interstitial ad full screen content opened.");
            };

            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("[AdMobInterstitialProvider] Interstitial ad full screen content closed.");
                _onAdCompleted?.Invoke();
                LoadAd();
            };

            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError($"[AdMobInterstitialProvider] Interstitial ad failed to open full screen content with error: {error}");
                _onAdFailed?.Invoke();
                LoadAd();
            };
        }

        public void Destroy()
        {
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
                Debug.Log("[AdMobInterstitialProvider] Interstitial ad destroyed.");
            }
        }
    }
}
