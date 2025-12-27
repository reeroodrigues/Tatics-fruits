using Managers;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class RemoveAdsButton : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private Button button;
        
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
            
            if (RemoveAdsManager.Instance != null)
            {
                RemoveAdsManager.Instance.OnRemovedAdsStatusChanged += OnRemoveAdsStatusChanged;
                UpdateButtonVisibility();
            }
        }

        private void OnButtonClicked()
        {
            
            if (IAPManager.Instance == null)
                return;
            

            if (!IAPManager.Instance.IsInitialized())
                return;

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

            if (_currentPopup != null)
            {
                _currentPopup.OnConfirm += OnPurchaseConfirmed;
                _currentPopup.OnCancel += OnPurchaseCancelled;
            }
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
            
            IAPManager.Instance.PurchaseRemoveAds();
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

        private void OnDestroy()
        {
            if(RemoveAdsManager.Instance != null)
                RemoveAdsManager.Instance.OnRemovedAdsStatusChanged -= OnRemoveAdsStatusChanged;
            
            ClosePopup();
        }
    }
}
