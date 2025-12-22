using System;
using Managers;
using UnityEngine;

namespace Ads
{
    public class InterstitialAdManager : MonoBehaviour
    {
        private const int MatchesBetweenAds = 3;
        private const int TestModeMatches = 1;

        [SerializeField] private bool enableAds = true;
        [SerializeField] private bool testMode = false;
        
        private IInterstitialAdProvider _adProvider;
        private bool _isShowingAd;
        private int _matchesSinceLastAd;

        public static InterstitialAdManager Instance { get; private set; }

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
            _matchesSinceLastAd = 0;
            LoadNextAd();
        }

        public void OnMatchCompleted()
        {
            if(!enableAds)
            {
                Debug.Log("[InterstitialAdManager] Ads are disabled.");
                return;
            }
    
            if(_isShowingAd)
            {
                Debug.Log("[InterstitialAdManager] Already showing an ad.");
                return;
            }

            _matchesSinceLastAd++;
    
            int currentInterval = testMode ? TestModeMatches : MatchesBetweenAds;
    
            Debug.Log($"[InterstitialAdManager] Match completed! Count: {_matchesSinceLastAd}/{currentInterval}. Provider: {(_adProvider != null ? "SET" : "NULL")}");

            if (_matchesSinceLastAd >= currentInterval)
            {
                ShowInterstitialAd();
            }
            else
            {
                Debug.Log($"[InterstitialAdManager] {currentInterval - _matchesSinceLastAd} more matches until next ad.");
            }
        }

        public void SetAdProvider(IInterstitialAdProvider provider)
        {
            _adProvider = provider;
            Debug.Log($"[InterstitialAdManager] ✅ Ad provider SET: {provider.GetType().Name}");
            LoadNextAd();
        }

        public void ShowInterstitialAd()
        {
            if (RemoveAdsManager.Instance != null && RemoveAdsManager.Instance.AreAdsRemoved())
            {
                return;
            }
            
            if (!enableAds || _isShowingAd)
            {
                Debug.Log("[InterstitialAdManager] Ads disabled or already showing an ad.");
                return;
            }

            if (_adProvider == null)
            {
                Debug.LogWarning("[InterstitialAdManager] No ad provider set. Please implement and set an IInterstitialAdProvider.");
                return;
            }

            if (!_adProvider.IsAdReady())
            {
                Debug.Log("[InterstitialAdManager] Interstitial ad is not ready yet. Loading next ad...");
                LoadNextAd();
                return;
            }

            Debug.Log("[InterstitialAdManager] Showing interstitial ad...");
            _isShowingAd = true;
            Time.timeScale = 0f;
            
            Managers.AnalyticsManager.Instance?.TrackAdStarted("interstitial", "auto");

            _adProvider.ShowAd(
                onAdCompleted: OnAdCompleted,
                onAdFailed: OnAdFailed
            );
        }

        private void OnAdCompleted()
        {
            Debug.Log("[InterstitialAdManager] Interstitial ad completed successfully.");
            Managers.AnalyticsManager.Instance?.TrackAdCompleted("interstitial", true);
            ResetAdTimer();
            ResumeGame();
            LoadNextAd();
        }

        private void OnAdFailed()
        {
            Debug.LogWarning("[InterstitialAdManager] Interstitial ad failed to show.");
            Managers.AnalyticsManager.Instance?.TrackAdCompleted("interstitial", false);
            ResetAdTimer();
            ResumeGame();
            LoadNextAd();
        }

        private void ResetAdTimer()
        {
            _matchesSinceLastAd = 0;
        }

        private void ResumeGame()
        {
            _isShowingAd = false;
            Time.timeScale = 1f;
        }

        private void LoadNextAd()
        {
            _adProvider?.LoadAd();
        }

        public void EnableAds(bool enable)
        {
            enableAds = enable;
        }

        public int GetMatchesUntilNextAd()
        {
            int currentInterval = testMode ? TestModeMatches : MatchesBetweenAds;
            return Mathf.Max(0, currentInterval - _matchesSinceLastAd);
        }

        public void ToggleTestMode()
        {
            testMode = !testMode;
            ResetAdTimer();
            Debug.Log($"[InterstitialAdManager] Test mode {(testMode ? "ENABLED" : "DISABLED")}. Interval: {(testMode ? TestModeMatches : MatchesBetweenAds)} matches");
        }

        public bool IsTestModeEnabled()
        {
            return testMode;
        }

        public void ForceShowAd()
        {
            ResetAdTimer();
            ShowInterstitialAd();
        }
    }
}
