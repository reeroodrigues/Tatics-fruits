using System;
using DG.Tweening;
using Gameplay.Utils;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace UI.Popups
{
    public class RemoveAdsPurchasePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform popupPanel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button closeButton;
        
        [Header("UI Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI confirmButtonText;
        [SerializeField] private TextMeshProUGUI cancelButtonText;
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        
        public event Action OnConfirm;
        public event Action OnCancel;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            SetupButtons();
            SetupTexts();
        }

        private void Start()
        {
            AnimateIn();

            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
                IAPManager.Instance.OnPurchaseFailed += OnPurchaseFailed;
            }
        }

        private void SetupButtons()
        {
            if(confirmButton != null)
                confirmButton.onClick.AddListener(HandleConfirm);
            
            if(cancelButton != null)
                cancelButton.onClick.AddListener(HandleCancel);

            if (closeButton != null)
                closeButton.onClick.AddListener(HandleCancel);
        }

        private void SetupTexts()
        {
            if (titleText != null)
                titleText.text = Localizer.Instance.Tr("remove_ads_title", "Remove Ads");
            
            if (descriptionText != null)
                descriptionText.text = Localizer.Instance.Tr("remove_ads_description", "Remove all ads from the game forever!\n\nEnjoy uninterrupted gameplay.");

            if (IAPManager.Instance != null && priceText != null)
            {
                var price = IAPManager.Instance.GetProductPrice();
                priceText.text = price;
            }

            if (confirmButtonText != null)
                confirmButtonText.text = Localizer.Instance.Tr("purchase_button", "Purhcase");

            if (cancelButtonText != null)
                cancelButtonText.text = Localizer.Instance.Tr("cancel_button", "Cancel");
        }

        private void HandleConfirm()
        {
            SetButtonsInteractable(false);
            
            if (confirmButtonText != null)
                confirmButtonText.text = Localizer.Instance.Tr("processing", "Processing...");
            
            OnConfirm?.Invoke();
        }

        private void HandleCancel()
        {
            OnCancel?.Invoke();
            AnimateOut();
        }

        private void OnPurchaseSuccess()
        {
            Debug.Log("[RemoveAdsPurchasePopup] Purchase successful! Restarting game...");
            
            if (descriptionText != null)
                descriptionText.text = Localizer.Instance.Tr("purchase_successful","Purchase successful!\nRestarting game...");

            DOVirtual.DelayedCall(1.5f, () =>
            {
                RestartGame();
            });
        }

        private void OnPurchaseFailed(string error)
        {
            Debug.LogWarning($"[RemoveAdsPurchasePopup] Purchase failed: {error}");
            
            if (descriptionText != null)
                descriptionText.text = Localizer.Instance.Tr("purchase_failed", $"Purchase failed:\n{error}") ;

            SetButtonsInteractable(true);

            DOVirtual.DelayedCall(2f, () =>
            {
                if (descriptionText != null)
                    descriptionText.text = Localizer.Instance.Tr("remove_ads_description", "Purchase successful!");
            });
        }

        private void RestartGame()
        {
            Debug.Log("[RemoveAdsPurchasePopup] Restarting application...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
            System.Diagnostics.Process.Start(Application.dataPath.Replace("_Data", ".exe"));
#endif
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (confirmButton != null)
                confirmButton.interactable = interactable;
            
            if (cancelButton != null)
                cancelButton.interactable = interactable;
            
            if (closeButton != null)
                closeButton.interactable = interactable;
        }

        private void AnimateIn()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, animationDuration).SetUpdate(true);
            }

            if (popupPanel != null)
            {
                popupPanel.localScale = Vector3.zero;
                popupPanel.DOScale(1f, animationDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }

        private void AnimateOut()
        {
            if (popupPanel != null)
            {
                popupPanel.DOScale(0f, animationDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true);
            }

            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, animationDuration * 0.7f)
                    .SetUpdate(true)
                    .OnComplete(() => Destroy(gameObject));
            }
            else
            {
                DOVirtual.DelayedCall(animationDuration * 0.7f, () => Destroy(gameObject)).SetUpdate(true);
            }
        }

        private void OnDestroy()
        {
            if (IAPManager.Instance != null)
            {
                IAPManager.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
                IAPManager.Instance.OnPurchaseFailed -= OnPurchaseFailed;
            }

            DOTween.Kill(this);
        }
    }
}
