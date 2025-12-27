using DG.Tweening;
using Managers;
using UI.Popups;
using UnityEngine;

namespace UI.Views
{
    public class RemoveAdsAutoPrompt : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private RemoveAdsPurchasePopup purchasePopupPrefab;
        [SerializeField] private Transform popupParent;
        
        [Header("Timing")]
        [SerializeField] private float delayBeforeShowing = 1.5f;

        [Header("Conditions")]
        [SerializeField] private bool showOnFirstLaunch = true;
        [SerializeField] private bool showOnReturnFromGameplay = true;
        
        private RemoveAdsPurchasePopup _currentPopup;

        private void Start()
        {
            var tracker = GameSessionTracker.Instance;
            
            if (ShouldShowPopup())
                DOVirtual.DelayedCall(delayBeforeShowing, ShowPopup);
        }

        private bool ShouldShowPopup()
        {
            if (RemoveAdsManager.Instance == null)
                return false;

            if (RemoveAdsManager.Instance.AreAdsRemoved())
                return false;

            var justReturned = GameSessionTracker.JustReturnedFromGameplay;
            var hasPlayed = GameSessionTracker.HasPlayedGameplayThisSession;
            

            if (showOnReturnFromGameplay && justReturned)
                return true;

            if (showOnFirstLaunch && !hasPlayed)
                return true;
            
            return false;
        }

        private void ShowPopup()
        {
            if (_currentPopup != null)
                return;
            
            if(purchasePopupPrefab == null)
                return;
            
            var parent = popupParent != null ? popupParent : transform.root;
            
            _currentPopup = Instantiate(purchasePopupPrefab, parent);

            if (_currentPopup != null)
            {
                _currentPopup.OnConfirm += OnPurchaseConfirmed;
                _currentPopup.OnCancel += OnPurchaseCancelled;
            }
            
            GameSessionTracker.ResetReturnedFlag();
        }

        private void OnPurchaseConfirmed()
        {
            if (IAPManager.Instance != null) 
                IAPManager.Instance.PurchaseRemoveAds();
        }

        private void OnPurchaseCancelled()
        {
            ClosePopup();
        }

        private void ClosePopup()
        {
            if (_currentPopup != null)
            {
                _currentPopup.OnConfirm -= OnPurchaseConfirmed;
                _currentPopup.OnCancel -= OnPurchaseCancelled;
                _currentPopup = null;
            }
        }

        private void OnDestroy()
        {
            ClosePopup();
        }
    }
}