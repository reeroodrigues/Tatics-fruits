using New_GameplayCore.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace New_GameplayCore.Views
{
    public class ShopCoinItemView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Button buyButton;

        private string _productId;
        private CoinPackSo _so;
        private ShopScript _shop;

        public void Setup(CoinPackSo so, ShopScript shop)
        {
            _so = so;
            _shop = shop;
            _productId = so.productId;
            
            if(icon) icon.sprite = so.icon;
            if (title) title.text = so.displayName;
            if (amountText) amountText.text = $"{so.coinAmount} Coins";

            RefreshPriceLabel();
            
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        private void RefreshPriceLabel()
        {
            if(!priceText)
                return;

            var storePrice = _shop != null ? _shop.GetLocalizedPrice(_productId, _so.priceText) : _so.priceText;
        }

        private void OnBuyClicked()
        {
            if(_shop == null)
                return;
            _shop.InitiatePurchase(_productId);
        }
    }
}