using New_GameplayCore.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
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
        private string _storeDescription;
        private IAPManager _iapManager;

        public void Setup(Product product, CoinPackSo packData, IAPManager manager)
        {
            _iapManager = manager;
            _productId = product.definition.id;
            
            if (icon) icon.sprite = packData.icon;
            if (title) title.text = packData.displayName;
            
            if (amountText) amountText.text = packData.coinAmount.ToString("N0"); 
            
            if (priceText) priceText.text = product.metadata.localizedPriceString; 
            
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
            
            buyButton.interactable = product.availableToPurchase;
        }

        private void OnBuyClicked()
        {
            if (_iapManager != null && !string.IsNullOrEmpty(_productId))
            {
                _iapManager.BuyProduct(_productId);
            }
        }
    }
}
