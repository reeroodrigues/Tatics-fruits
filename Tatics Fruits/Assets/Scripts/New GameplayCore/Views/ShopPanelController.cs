using System.Collections.Generic;
using System.Linq;
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

        // Esse CanvasGroup deve estar NO ROOT do painel da loja
        // e o mesmo GameObject precisa ter um Image para bloquear cliques.
        [SerializeField] private CanvasGroup panelCanvasGroup;

        [Header("Coin Pack")]
        [SerializeField] private List<CoinPackSo> coinPacks;

        [Header("Special Buttons")] 
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Button vipButton;
        
        private void Awake()
        {
            // Garante que começa fechado
            if (panelCanvasGroup)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void Start()
        {
            SetupSpecialButtons();
            PopulateCoinPacks();

            if (closeButton)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Hide);
            }
        }
        
        public void Show()
        {
            // Ativa o GO antes de mexer no CanvasGroup
            gameObject.SetActive(true);

            if (panelCanvasGroup)
            {
                panelCanvasGroup.alpha = 1f;
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            if (panelCanvasGroup)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        private void SetupSpecialButtons()
        {
            if (removeAdsButton)
            {
                removeAdsButton.onClick.RemoveAllListeners();
                removeAdsButton.onClick.AddListener(() =>
                {
                    if (shopScript != null)
                        shopScript.InitiatePurchase(shopScript.ncItem.id);
                });
            }
            
            if (vipButton)
            {
                vipButton.onClick.RemoveAllListeners();
                vipButton.onClick.AddListener(() =>
                {
                    if (shopScript != null)
                        shopScript.InitiatePurchase(shopScript.sItem.id);
                });
            }
        }

        private void PopulateCoinPacks()
        {
            if (!coinPackContainer || !coinItemPrefab) return;

            foreach (Transform child in coinPackContainer)
                Destroy(child.gameObject);

            var ordered = coinPacks
                .OrderBy(p => p.size)
                .ToList();

            foreach (var pack in ordered)
            {
                var item = Instantiate(coinItemPrefab, coinPackContainer);
                item.Setup(pack, shopScript);
            }
        }
    }
}
