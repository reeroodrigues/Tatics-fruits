using System;
using DG.Tweening;
using Gameplay.Utils;
using Managers;
using TMPro;
using UnityEngine;
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
        
        [Header("Discount UI")]
        [SerializeField] private TextMeshProUGUI originalPriceText;
        [SerializeField] private TextMeshProUGUI savingsText;
        [SerializeField] private TextMeshProUGUI discountBadgeText;
        [SerializeField] private TextMeshProUGUI urgencyText;
        [SerializeField] private TextMeshProUGUI bannerText;
        [SerializeField] private RectTransform discountBadge;
        [SerializeField] private RectTransform limitedTimeBanner;
        
        [Header("Discount Settings")]
        [SerializeField] private bool showDiscount = true;
        [SerializeField] private int discountPercentage = 50;
        [SerializeField] private string originalPriceOverride = "";
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private float badgePulseScale = 1.1f;
        [SerializeField] private float badgePulseDuration = 0.8f;
        
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
                descriptionText.text = Localizer.Instance.Tr("remove_ads_description", "Remove all ads from the game forever!");

            if (bannerText != null)
                bannerText.text = Localizer.Instance.Tr("limited_time_banner", "LIMITED TIME OFFER");

            if (urgencyText != null)
                urgencyText.text = Localizer.Instance.Tr("offer_ends_soon", "Offer ends soon!");

            SetupPricing();

            if (confirmButtonText != null)
                confirmButtonText.text = Localizer.Instance.Tr("purchase_button", "BUY NOW");
        }

        private void SetupPricing()
        {
            if (IAPManager.Instance == null)
                return;

            string currentPrice = IAPManager.Instance.GetProductPrice();

            if (priceText != null)
                priceText.text = currentPrice;

            if (showDiscount)
            {
                if (originalPriceText != null)
                {
                    string originalPrice = !string.IsNullOrEmpty(originalPriceOverride) 
                        ? originalPriceOverride 
                        : CalculateOriginalPrice(currentPrice);
                    
                    originalPriceText.text = $"<s>{originalPrice}</s>";
                }

                if (discountBadgeText != null)
                    discountBadgeText.text = $"-{discountPercentage}%";

                if (savingsText != null)
                {
                    string savingsLabel = Localizer.Instance.Tr("you_save", "You save");
                    string savings = CalculateSavings(currentPrice);
                    savingsText.text = $"{savingsLabel} {savings}!";
                }
            }
            else
            {
                if (originalPriceText != null) originalPriceText.gameObject.SetActive(false);
                if (savingsText != null) savingsText.gameObject.SetActive(false);
                if (discountBadge != null) discountBadge.gameObject.SetActive(false);
                if (limitedTimeBanner != null) limitedTimeBanner.gameObject.SetActive(false);
                if (urgencyText != null) urgencyText.gameObject.SetActive(false);
            }
        }

        private string CalculateOriginalPrice(string currentPrice)
        {
            string numericPart = System.Text.RegularExpressions.Regex.Replace(currentPrice, @"[^\d,.]", "");
            
            if (float.TryParse(numericPart.Replace(",", "."), System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out float price))
            {
                float originalPrice = price / (1f - (discountPercentage / 100f));
                string currency = currentPrice.Replace(numericPart, "").Trim();
                
                if (currentPrice.Contains(","))
                    return $"{currency} {originalPrice:F2}".Replace(".", ",");
                else
                    return $"{currency} {originalPrice:F2}";
            }

            return currentPrice;
        }

        private string CalculateSavings(string currentPrice)
        {
            string numericPart = System.Text.RegularExpressions.Regex.Replace(currentPrice, @"[^\d,.]", "");
            
            if (float.TryParse(numericPart.Replace(",", "."), System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out float price))
            {
                float savings = price * (discountPercentage / 100f) / (1f - (discountPercentage / 100f));
                string currency = currentPrice.Replace(numericPart, "").Trim();
                
                if (currentPrice.Contains(","))
                    return $"{currency} {savings:F2}".Replace(".", ",");
                else
                    return $"{currency} {savings:F2}";
            }

            return "";
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
                descriptionText.text = Localizer.Instance.Tr("purchase_success", "Purchase successful!\nRestarting game...");

            DOVirtual.DelayedCall(1.5f, () =>
            {
                RestartGame();
            });
        }

        private void OnPurchaseFailed(string error)
        {
            Debug.LogWarning($"[RemoveAdsPurchasePopup] Purchase failed: {error}");
            
            if (descriptionText != null)
            {
                string failedMessage = Localizer.Instance.Tr("purchase_failed", "Purchase failed");
                descriptionText.text = $"{failedMessage}:\n{error}";
            }

            SetButtonsInteractable(true);

            if (confirmButtonText != null)
                confirmButtonText.text = Localizer.Instance.Tr("purchase_button", "BUY NOW");

            DOVirtual.DelayedCall(2f, () =>
            {
                if (descriptionText != null)
                    descriptionText.text = Localizer.Instance.Tr("remove_ads_description", "Remove all ads from the game forever!\nEnjoy uninterrupted gameplay.");
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

            if (discountBadge != null && showDiscount)
            {
                discountBadge.localScale = Vector3.zero;
                discountBadge.DOScale(1f, animationDuration * 1.5f)
                    .SetEase(Ease.OutElastic)
                    .SetDelay(animationDuration * 0.5f)
                    .SetUpdate(true)
                    .OnComplete(() => PulseBadge());
            }

            if (limitedTimeBanner != null && showDiscount)
            {
                limitedTimeBanner.DOShakeRotation(0.5f, new Vector3(0, 0, 10f), 10, 90)
                    .SetDelay(animationDuration)
                    .SetUpdate(true);
            }
        }

        private void PulseBadge()
        {
            if (discountBadge == null) return;

            discountBadge.DOScale(badgePulseScale, badgePulseDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
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
            
            if (discountBadge != null)
                discountBadge.DOKill();
        }
    }
}
