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
        private ShopScript _shop;

        public void Setup(CoinPackSo so, ShopScript shop)
        {
            _shop = shop;
            _productId = so.productId;
            
            if(icon) icon.sprite = so.icon;
            if (title) title.text = so.displayName;
            if (amountText) amountText.text = $"{so.coinAmount} Coins";
            if (priceText) priceText.text = so.priceText;
            
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                _shop.InitiatePurchase(_productId);
            });
        }

    }
}