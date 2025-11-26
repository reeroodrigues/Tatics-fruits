using GoogleMobileAds.Api;
using UnityEngine;
using Core;

namespace Ads
{
    public class GoogleMobileAdsInitializer : MonoBehaviour
    {
        private static bool _isInitialized = false;

        private void Start()
        {
            InitializeGoogleMobileAds();
        }

        private void InitializeGoogleMobileAds()
        {
            if (_isInitialized)
            {
                DebugLogger.Log("[GoogleMobileAdsInitializer] Google Mobile Ads SDK already initialized.");
                return;
            }

            DebugLogger.Log("[GoogleMobileAdsInitializer] 🚀 INITIALIZING GOOGLE MOBILE ADS SDK...");
            DebugLogger.Log($"[GoogleMobileAdsInitializer] Platform: {Application.platform}");
            DebugLogger.Log($"[GoogleMobileAdsInitializer] Internet Reachability: {Application.internetReachability}");

            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                if (initStatus == null)
                {
                    DebugLogger.LogError("[GoogleMobileAdsInitializer] ❌ INITIALIZATION FAILED - Status is NULL!");
                    return;
                }

                _isInitialized = true;
                DebugLogger.Log("[GoogleMobileAdsInitializer] ✅ INITIALIZATION COMPLETE!");

                var adapterStatusMap = initStatus.getAdapterStatusMap();
                foreach (var item in adapterStatusMap)
                {
                    string statusIcon = item.Value.InitializationState == GoogleMobileAds.Api.AdapterState.Ready ? "✅" : "⚠️";
                    DebugLogger.Log($"[GoogleMobileAdsInitializer] {statusIcon} Adapter: {item.Key}");
                    DebugLogger.Log($"[GoogleMobileAdsInitializer]    Status: {item.Value.InitializationState}");
                    DebugLogger.Log($"[GoogleMobileAdsInitializer]    Latency: {item.Value.Latency}ms");
                    DebugLogger.Log($"[GoogleMobileAdsInitializer]    Description: {item.Value.Description}");
                }
            });
        }

        public static bool IsInitialized()
        {
            return _isInitialized;
        }
    }
}
