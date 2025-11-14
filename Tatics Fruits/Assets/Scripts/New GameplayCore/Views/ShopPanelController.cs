using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using New_GameplayCore.Services;
using UnityEngine;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class ShopPanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ShopScript shopScript;
        [SerializeField] private Transform coinPackContainer;
        [SerializeField] private ShopCoinItemView coinItemPrefab;
        [SerializeField] private Button closeButton;
        
        [Header("UI Blocking")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private Image backgroundBlocker;

        [Header("Window Animation")]
        [SerializeField] private RectTransform window;

        [Header("Coin Packs")]
        [SerializeField] private List<CoinPackSo> coinPacks;

        [Header("Special Buttons")]
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Button vipButton;

        private bool _animating;

        private void Awake()
        {
            if (panelCanvasGroup)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.blocksRaycasts = false;
                panelCanvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

        private void Start()
        {
            SetupSpecialButtons();
            PopulateCoinPacks();

            if (closeButton)
                closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            if (_animating) return;
            _animating = true;

            gameObject.SetActive(true);
            
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.blocksRaycasts = true;
            panelCanvasGroup.interactable = true;

            if (backgroundBlocker)
                backgroundBlocker.raycastTarget = true;
            
            if (window)
            {
                window.localScale = new Vector3(0.8f, 0.8f, 1f);

                DOTween.Sequence()
                    .Append(window.DOScale(1f, 0.22f).SetEase(Ease.OutBack))
                    .OnComplete(() => _animating = false);
            }
            else
            {
                _animating = false;
            }
        }

        public void Hide()
        {
            if (_animating) return;
            _animating = true;
            
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;

            if (backgroundBlocker)
                backgroundBlocker.raycastTarget = false;

            DOTween.Sequence()
                .Append(panelCanvasGroup.DOFade(0f, 0.15f))
                .Join(window ? window.DOScale(0.94f, 0.15f).SetEase(Ease.InSine) : null)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _animating = false;
                });
        }

        private void SetupSpecialButtons()
        {
            removeAdsButton.onClick.RemoveAllListeners();
            removeAdsButton.onClick.AddListener(() =>
            {
                shopScript.InitiatePurchase(shopScript.ncItem.id);
            });

            vipButton.onClick.RemoveAllListeners();
            vipButton.onClick.AddListener(() =>
            {
                shopScript.InitiatePurchase(shopScript.sItem.id);
            });
        }

        private void PopulateCoinPacks()
        {
            foreach (Transform child in coinPackContainer)
                Destroy(child.gameObject);

            var ordered = coinPacks.OrderBy(p => p.size).ToList();

            foreach (var pack in ordered)
            {
                var item = Instantiate(coinItemPrefab, coinPackContainer);
                item.Setup(pack, shopScript);
            }
        }
    }
}
