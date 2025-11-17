using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace New_GameplayCore.Services
{
    public class IAPManager : MonoBehaviour, IStoreListener
    {
        private IStoreController _controller;
        private IExtensionProvider _extension;
        
        [SerializeField] private PlayerProfileController playerProfileController;

        public const string ProductCoinPack50 = "coin_50";
        public const string ProductCoinPack100 = "coin_100";
        public const string ProductCoinPack200 = "coin_200";
        public const string ProductRemoveAds = "no_ad"; 
        public const string ProductVIP = "r_pass";
        public Action<IEnumerable<Product>> onIAPInitialized;

        private void Start()
        {
            InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            if(IsInitialized())
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            
            builder.AddProduct(ProductCoinPack50, ProductType.Consumable);
            builder.AddProduct(ProductCoinPack100, ProductType.Consumable);
            builder.AddProduct(ProductCoinPack200, ProductType.Consumable);
            
            builder.AddProduct(ProductRemoveAds, ProductType.NonConsumable);
            
            builder.AddProduct(ProductVIP, ProductType.Subscription);
    
            UnityPurchasing.Initialize(this, builder);
        }

        public bool IsInitialized()
        {
            return _controller != null && _extension != null;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extension)
        {
            _controller = controller;
            _extension = extension;
            
            Debug.Log("IAP Inicializado. Notificando Views.");
            
            onIAPInitialized?.Invoke(controller.products.all);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogError("Falha na inicialização do IAP: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message = null)
        {
            throw new NotImplementedException();
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var productID = args.purchasedProduct.definition.id;

            if (string.Equals(productID, ProductCoinPack50, StringComparison.Ordinal))
            {
                playerProfileController.AddGoldAndSave(50);
                Debug.Log("50 Moedas adicionadas.");
            }
            else if (string.Equals(productID, ProductCoinPack100, StringComparison.Ordinal))
            {
                playerProfileController.AddGoldAndSave(100);
                Debug.Log("100 Moedas adicionadas.");
            }
            else if (string.Equals(productID, ProductCoinPack200, StringComparison.Ordinal))
            {
                playerProfileController.AddGoldAndSave(200);
                Debug.Log("200 Moedas adicionadas.");
            }
            return PurchaseProcessingResult.Complete;
        }

        public void BuyProduct(string productID)
        {
            if (IsInitialized())
            {
                var product = _controller.products.WithID(productID);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log($"Comprando produto:'{product.definition.id}'");
                    _controller.InitiatePurchase(product);
                }
                else
                {
                    Debug.LogError("Produto não encontrado ou indisponível para compra.");
                }
            }
            else
            {
                Debug.LogError("IAP não inicializado");
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogError($"Falha na compra do produto {product.definition.id}. Motivo: {failureReason}");
        }

        public void RestorePurchase()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
            }
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _controller?.products.all;
        }
    }
}