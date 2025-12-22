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
        
        [Header("Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        
        public event Action OnConfirm;
        public event Action OnCancel;
        private Localizer _localizer;

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
                titleText.text = _localizer.Tr("remove_ads_title");
            
            if (descriptionText != null)
                descriptionText.text = _localizer.Tr("remove_ads_description");

            if (IAPManager.Instance != null && priceText != null)
            {
                var price = IAPManager.Instance.GetProductPrice();
                priceText.text = price;
            }

            if (confirmButtonText != null)
                confirmButtonText.text = _localizer.Tr("remove_ads_confirm");
        }

        private void HandleConfirm()
        {
            SetButtonsInteractable(false);
            OnConfirm?.Invoke();
        }

        private void HandleCancel()
        {
            OnCancel?.Invoke();
            AnimateOut();
        }

        private void OnPurchaseSuccess()
        {
            if (descriptionText != null)
                descriptionText.text = _localizer.Tr("remove_ads_description_success");

            DOVirtual.DelayedCall(1.5f, () =>
            {
                RestartGame();
            });
        }

        private void OnPurchaseFailed(string error)
        {
            if (descriptionText != null)
                descriptionText.text = _localizer.Tr($"remove_ads_description_failed: {error}");

            SetButtonsInteractable(true);

            DOVirtual.DelayedCall(2f, () =>
            {
                if (descriptionText != null)
                    descriptionText.text =  _localizer.Tr("remove_ads_description_failed");
            });
        }

        private void RestartGame()
        {
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