using System;
using UnityEngine;

namespace Ads
{
    public class InterstitialAdManager : MonoBehaviour
    {
        private const float AD_INTERVAL_SECONDS = 180f;
        private const float TEST_MODE_INTERVAL_SECONDS = 30f;

        [SerializeField] private bool enableAds = true;
        [SerializeField] private bool testMode = false;
        
        private IInterstitialAdProvider _adProvider;
        private float _timeSinceLastAd;
        private bool _isShowingAd;

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
            _timeSinceLastAd = 0f;
            LoadNextAd();
        }

        private void Update()
        {
            if (!enableAds || _isShowingAd || _adProvider == null) return;

            _timeSinceLastAd += Time.deltaTime;

            float currentInterval = testMode ? TEST_MODE_INTERVAL_SECONDS : AD_INTERVAL_SECONDS;

            if (_timeSinceLastAd >= currentInterval)
            {
                ShowInterstitialAd();
            }
        }

        public void SetAdProvider(IInterstitialAdProvider provider)
        {
            _adProvider = provider;
            LoadNextAd();
        }

        public void ShowInterstitialAd()
        {
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

            _adProvider.ShowAd(
                onAdCompleted: OnAdCompleted,
                onAdFailed: OnAdFailed
            );
        }

        private void OnAdCompleted()
        {
            Debug.Log("[InterstitialAdManager] Interstitial ad completed successfully.");
            ResetAdTimer();
            ResumeGame();
            LoadNextAd();
        }

        private void OnAdFailed()
        {
            Debug.LogWarning("[InterstitialAdManager] Interstitial ad failed to show.");
            ResetAdTimer();
            ResumeGame();
            LoadNextAd();
        }

        private void ResetAdTimer()
        {
            _timeSinceLastAd = 0f;
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

        public float GetTimeUntilNextAd()
        {
            float currentInterval = testMode ? TEST_MODE_INTERVAL_SECONDS : AD_INTERVAL_SECONDS;
            return Mathf.Max(0f, currentInterval - _timeSinceLastAd);
        }

        public void ToggleTestMode()
        {
            testMode = !testMode;
            ResetAdTimer();
            Debug.Log($"[InterstitialAdManager] Test mode {(testMode ? "ENABLED" : "DISABLED")}. Interval: {(testMode ? TEST_MODE_INTERVAL_SECONDS : AD_INTERVAL_SECONDS)}s");
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
