using System;
using System.Collections;
using UnityEngine;

namespace Ads
{
    public class TestAdProvider : MonoBehaviour, IInterstitialAdProvider
    {
        [Header("Test Settings")]
        [SerializeField] private float adDuration = 3f;
        [SerializeField] private bool simulateFailures = false;
        [SerializeField] [Range(0f, 1f)] private float failureRate = 0.2f;

        private bool _adReady = true;
        private Action _onAdCompleted;
        private Action _onAdFailed;

        public bool IsAdReady()
        {
            return _adReady;
        }

        public void ShowAd(Action onAdCompleted, Action onAdFailed)
        {
            _onAdCompleted = onAdCompleted;
            _onAdFailed = onAdFailed;

            if (simulateFailures && UnityEngine.Random.value < failureRate)
            {
                Debug.Log($"[TestAdProvider] Simulating ad failure ({failureRate * 100}% failure rate)");
                _adReady = false;
                StartCoroutine(SimulateAdFailure());
            }
            else
            {
                Debug.Log($"[TestAdProvider] Showing test ad for {adDuration} seconds...");
                _adReady = false;
                StartCoroutine(SimulateAd());
            }
        }

        public void LoadAd()
        {
            Debug.Log("[TestAdProvider] Loading test ad (instant)");
            _adReady = true;
        }

        private IEnumerator SimulateAd()
        {
            Debug.Log("[TestAdProvider] === AD STARTED ===");
            Debug.Log($"[TestAdProvider] Imagine an ad playing here for {adDuration} seconds...");
            
            yield return new WaitForSecondsRealtime(adDuration);
            
            Debug.Log("[TestAdProvider] === AD COMPLETED ===");
            _onAdCompleted?.Invoke();
        }

        private IEnumerator SimulateAdFailure()
        {
            yield return new WaitForSecondsRealtime(0.5f);
            
            Debug.LogWarning("[TestAdProvider] === AD FAILED ===");
            _onAdFailed?.Invoke();
        }

        public void SetAdDuration(float duration)
        {
            adDuration = duration;
        }

        public void SetSimulateFailures(bool enable, float rate = 0.2f)
        {
            simulateFailures = enable;
            failureRate = Mathf.Clamp01(rate);
        }
    }
}
