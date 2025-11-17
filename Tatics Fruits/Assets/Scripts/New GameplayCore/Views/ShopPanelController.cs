using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using New_GameplayCore.Services;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

namespace New_GameplayCore.Views
{
    public class ShopPanelController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private IAPManager shopScript;
        [SerializeField] private Transform coinPackContainer;
        [SerializeField] private ShopCoinItemView coinItemPrefab;
        [SerializeField] private Button closeButton;
        
        [Header("UI Blocking")]
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private Image backgroundBlocker;
        
        [Header("Coin Packs Data")]
        [SerializeField] private List<CoinPackSo> allCoinPacksData;

        [Header("Window Animation")]
        [SerializeField] private RectTransform window;

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

            if (shopScript != null)
            {
                if (shopScript.IsInitialized())
                {
                    PopulateCoinPacks(shopScript.GetAllProducts());
                }
                else
                {
                    shopScript.onIAPInitialized += PopulateCoinPacks;
                }
            }
    
            if(closeButton)
                closeButton.onClick.AddListener(Hide);
        }

        private void OnDestroy()
        {
            if (shopScript != null)
            {
                shopScript.onIAPInitialized -= PopulateCoinPacks;
            }
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
                shopScript.BuyProduct(IAPManager.ProductRemoveAds);
            });

            vipButton.onClick.RemoveAllListeners();
            vipButton.onClick.AddListener(() =>
            {
                shopScript.BuyProduct(IAPManager.ProductVIP);
            });
        }

        private void PopulateCoinPacks(IEnumerable<Product> availableProducts)
        {
            if (coinPackContainer == null || coinItemPrefab == null)
            {
                Debug.LogError("Referências de UI para Coin Packs estão faltando.");
                return;
            }

            foreach (Transform child in coinPackContainer)
                Destroy(child.gameObject);
            
            var dataMap = allCoinPacksData.ToDictionary(pack => pack.productId, pack => pack);
            
            var shopProducts = availableProducts
                .Where(p => p.definition.type == ProductType.Consumable)
                .OrderBy(p => p.metadata.localizedPrice)
                .ToList();

            foreach (var product in shopProducts)
            {
                if (dataMap.TryGetValue(product.definition.id, out CoinPackSo packData))
                {
                    var item = Instantiate(coinItemPrefab, coinPackContainer);
                    
                    item.Setup(product, packData, shopScript);
                }
                else
                {
                    Debug.LogWarning($"CoinPackSo não encontrado para o ID IAP: {product.definition.id}. Este item não será exibido.");
                }
            }
        }
    }
}
