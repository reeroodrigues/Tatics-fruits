using System;
using Ads;
using UnityEngine;

namespace Managers
{
    public class RemoveAdsManager : MonoBehaviour
    {
        public static RemoveAdsManager Instance { get; private set; }
        public event Action OnAdsRemoved;
        public event Action<bool> OnRemovedAdsStatusChanged;
        
        private DataSaver _dataSaver;
        private bool _adsRemoved = false;

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
            _dataSaver = FindObjectOfType<DataSaver>();

            if (_dataSaver == null)
                return;

            _dataSaver.OnDataLoaded += OnPlayerDataLoaded;
            _dataSaver.OnRemoteDataChanged += OnPlayerDataLoaded;
        }

        private void OnPlayerDataLoaded(DataToSave data)
        {
            if (data == null)
                return;

            var newStatus = data.removeAds;

            if (newStatus != _adsRemoved)
            {
                _adsRemoved = newStatus;
                ApplyAdRemovalStatus();
                OnRemovedAdsStatusChanged?.Invoke(_adsRemoved);
            }
        }

        public bool AreAdsRemoved()
        {
            return _adsRemoved;
        }
        
        public void UnlockRemoveAds()
        {
            if (_adsRemoved)
                return;
            
            _adsRemoved = true;

            if (_dataSaver != null)
            {
                _dataSaver.SetRemoveAds(true);
                _dataSaver.SaveData(force: true);
            }

            ApplyAdRemovalStatus();
            OnAdsRemoved?.Invoke();
            OnRemovedAdsStatusChanged?.Invoke(_adsRemoved);
        }

        private void ApplyAdRemovalStatus()
        {
            if (_adsRemoved)
            {
                DisableAllAds();
            }
            else
            {
                EnableAllAds();
            }
        }

        private void DisableAllAds()
        {
            if (BannerAdManager.Instance != null)
                BannerAdManager.Instance.enabled = false;

            if (InterstitialAdManager.Instance != null)
                InterstitialAdManager.Instance.enabled = false;
        }

        private void EnableAllAds()
        {
            if (BannerAdManager.Instance != null)
                BannerAdManager.Instance.enabled = true;
            
            if (InterstitialAdManager.Instance != null)
                InterstitialAdManager.Instance.enabled = true;
        }

        public void RestorePurchase()
        {
            if (_dataSaver != null && _dataSaver.dataToSave != null)
            {
                _adsRemoved = _dataSaver.dataToSave.removeAds;
                ApplyAdRemovalStatus();
            }
        }

        private void OnDestroy()
        {
            if (_dataSaver != null)
            {
                _dataSaver.OnDataLoaded -= OnPlayerDataLoaded;
                _dataSaver.OnRemoteDataChanged -= OnPlayerDataLoaded;
            }
        }
    }
}