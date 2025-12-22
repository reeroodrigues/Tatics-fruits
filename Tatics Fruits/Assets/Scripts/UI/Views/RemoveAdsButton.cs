using System;
using Managers;
using TMPro;
using UI.Popups;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class RemoveAdsButton : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private GameObject loadingIndicator;
        
        [Header("Popup")]
        [SerializeField] private RemoveAdsPurchasePopup purchasePopupPrefab;
        [SerializeField] private Transform popupParent;

        private RemoveAdsPurchasePopup _currentPopup;

        private void Awake()
        {
            if(button == null)
                button = GetComponent<Button>();
        }

        private void Start()
        {
            button.onClick.AddListener(OnButtonClicked);

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnInitializationComplete += UpdatePriceDisplay;
                UpdatePriceDisplay();
            }

            if (RemoveAdsManager.Instance != null)
            {
                RemoveAdsManager.Instance.OnRemovedAdsStatusChanged += OnRemoveAdsStatusChanged;
                UpdateButtonVisibility();
            }

            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
        }

        private void OnButtonClicked()
        {
            if (IAPManager.Instance == null)
                return;

            if (!IAPManager.Instance.IsInitialized())
            {
                ShowLoadingState(true);
                return;
            }

            ShowPurchasePopup();
        }

        private void ShowPurchasePopup()
        {
            if (_currentPopup != null)
                return;

            if (purchasePopupPrefab == null)
            {
                InitiatePurchaseDirectly();
                return;
            }

            var parent = popupParent != null ? popupParent : transform.root;
            _currentPopup = Instantiate(purchasePopupPrefab, parent);

            _currentPopup.OnConfirm += OnPurchaseConfirmed;
            _currentPopup.OnCancel += OnPurchaseCancelled;
        }

        private void OnPurchaseConfirmed()
        {
            InitiatePurchaseDirectly();
            ClosePopup();
        }

        private void OnPurchaseCancelled()
        {
            ClosePopup();
        }

        private void ClosePopup()
        {
            if (_currentPopup != null)
            {
                Destroy(_currentPopup.gameObject);
                _currentPopup = null;
            }
        }

        private void InitiatePurchaseDirectly()
        {
            if(IAPManager.Instance == null)
                return;

            ShowLoadingState(true);
            IAPManager.Instance.PurchaseRemoveAds();
        }

        private void UpdatePriceDisplay()
        {
            if(priceText == null || IAPManager.Instance == null)
                return;

            var price = IAPManager.Instance.GetProductPrice();
            priceText.text = price;
        }

        private void OnRemoveAdsStatusChanged(bool adsRemoved)
        {
            UpdateButtonVisibility();
        }

        private void UpdateButtonVisibility()
        {
            if(RemoveAdsManager.Instance == null)
                return;

            var adsRemoved = RemoveAdsManager.Instance.AreAdsRemoved();
            gameObject.SetActive(!adsRemoved);
        }

        private void ShowLoadingState(bool isLoading)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(isLoading);
            
            if(button != null)
                button.interactable = !isLoading;
        }

        private void OnDestroy()
        {
            if (IAPManager.Instance != null)
                IAPManager.Instance.OnInitializationComplete -= UpdatePriceDisplay;
            
            if(RemoveAdsManager.Instance != null)
                RemoveAdsManager.Instance.OnRemovedAdsStatusChanged -= OnRemoveAdsStatusChanged;
            
            ClosePopup();
        }
        
    }
}