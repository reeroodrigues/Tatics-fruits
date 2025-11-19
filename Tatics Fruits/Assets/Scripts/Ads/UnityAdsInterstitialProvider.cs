using System;
using UnityEngine;

namespace Ads
{
    public class UnityAdsInterstitialProvider : IInterstitialAdProvider
    {
        private const string ANDROID_AD_UNIT_ID = "Interstitial_Android";
        private const string IOS_AD_UNIT_ID = "Interstitial_iOS";

        private string _adUnitId;
        private bool _adReady;
        private Action _onAdCompleted;
        private Action _onAdFailed;

        public UnityAdsInterstitialProvider()
        {
#if UNITY_ANDROID
            _adUnitId = ANDROID_AD_UNIT_ID;
#elif UNITY_IOS
            _adUnitId = IOS_AD_UNIT_ID;
#else
            _adUnitId = ANDROID_AD_UNIT_ID;
#endif
        }

        public bool IsAdReady()
        {
            return _adReady;
        }

        public void ShowAd(Action onAdCompleted, Action onAdFailed)
        {
            _onAdCompleted = onAdCompleted;
            _onAdFailed = onAdFailed;

            Debug.Log($"Unity Ads: Showing interstitial ad with ID: {_adUnitId}");
            
            _adReady = false;
        }

        public void LoadAd()
        {
            Debug.Log($"Unity Ads: Loading interstitial ad with ID: {_adUnitId}");
            _adReady = true;
        }
    }
}
