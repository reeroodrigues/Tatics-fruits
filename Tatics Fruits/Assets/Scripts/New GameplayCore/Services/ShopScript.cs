using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;

namespace New_GameplayCore.Services
{
    [Serializable]
    public class ConsumableItem
    {
        public string name;
        public string id;
        public string description;
        public float price;
    }

    [Serializable]
    public class NonConsumableItem
    {
        public string name;
        public string id;
        public string description;
        public float price;
    }

    [Serializable]
    public class SubscriptionItem
    {
        public string name;
        public string id;
        public string description;
        public float price;
        public int timeDuration; // in days
    }

    /// <summary>
    /// Informações completas de um produto retornado pela Google Play / App Store.
    /// </summary>
    [Serializable]
    public class ProductInfo
    {
        public string id;
        public ProductType type;
        public string name;
        public string description;
        public string priceString;
        public decimal priceValue;
        public string isoCurrencyCode;
    }

    public class ShopScript : MonoBehaviour, IDetailedStoreListener
    {
        [Header("Coin Packs (consumíveis)")]
        public List<CoinPackSo> coinPackList;

        [Header("Produtos especiais")]
        public NonConsumableItem ncItem;   // ex: Remove Ads
        public SubscriptionItem sItem;     // ex: VIP / Elite Pass

        public IStoreController StoreController { get; private set; }
        private IExtensionProvider _extensions;

        // Cache de produtos retornados pela loja
        private readonly Dictionary<string, ProductInfo> _products = new();

        [Header("UI de moedas (opcional)")]
        public TextMeshProUGUI coinTxt;

        private void Start()
        {
            // Atualiza texto de moedas (opcional)
            if (coinTxt != null)
            {
                var coins = PlayerPrefs.GetInt("totalCoins");
                coinTxt.text = coins.ToString();
            }

            InitializePurchasing();
        }

        #region Inicialização IAP

        private void InitializePurchasing()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Consumíveis (coin packs)
            if (coinPackList != null)
            {
                foreach (var pack in coinPackList)
                {
                    if (!string.IsNullOrEmpty(pack.productId))
                    {
                        builder.AddProduct(pack.productId, ProductType.Consumable);
                    }
                }
            }

            // Não-consumível (ex: remove ads)
            if (!string.IsNullOrEmpty(ncItem.id))
            {
                builder.AddProduct(ncItem.id, ProductType.NonConsumable);
            }

            // Assinatura (opcional)
            if (!string.IsNullOrEmpty(sItem.id))
            {
                builder.AddProduct(sItem.id, ProductType.Subscription);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP initialized successfully");
            StoreController = controller;
            _extensions = extensions;

            CacheProducts(controller.products);
            
            SyncSpecialItems();
            CheckNonConsumable(ncItem.id);
            CheckSubscription(sItem.id);
        }

        /// <summary>
        /// Lê todas as infos vindas da Google Play e guarda no dicionário _products.
        /// </summary>
        private void CacheProducts(ProductCollection productCollection)
        {
            _products.Clear();

            foreach (var product in productCollection.all)
            {
                if (product == null || product.metadata == null)
                    continue;

                _products[product.definition.id] = new ProductInfo
                {
                    id = product.definition.id,
                    type = product.definition.type,
                    name = product.metadata.localizedTitle,
                    description = product.metadata.localizedDescription,
                    priceString = product.metadata.localizedPriceString,
                    priceValue = product.metadata.localizedPrice,
                    isoCurrencyCode = product.metadata.isoCurrencyCode
                };
            }

            Debug.Log($"Cached {_products.Count} IAP products from store.");
        }

        /// <summary>
        /// Preenche ncItem e sItem com os dados vindos da loja.
        /// </summary>
        private void SyncSpecialItems()
        {
            if (!string.IsNullOrEmpty(ncItem.id) &&
                TryGetProductInfo(ncItem.id, out var nonConsumableInfo))
            {
                ncItem.name = nonConsumableInfo.name;
                ncItem.description = nonConsumableInfo.description;
                ncItem.price = (float)nonConsumableInfo.priceValue;
            }

            if (!string.IsNullOrEmpty(sItem.id) &&
                TryGetProductInfo(sItem.id, out var subInfo))
            {
                sItem.name = subInfo.name;
                sItem.description = subInfo.description;
                sItem.price = (float)subInfo.priceValue;
            }
        }

        #endregion

        #region APIs de leitura de produto

        /// <summary>
        /// Retorna o preço formatado (ex: "R$ 4,90") do produto.
        /// </summary>
        public string GetLocalizedPrice(string productId, string fallback = "")
        {
            if (TryGetProductInfo(productId, out var info))
                return info.priceString;

            return fallback;
        }

        /// <summary>
        /// Retorna todas as infos (id, nome, descrição, preço) de um produto.
        /// </summary>
        public bool TryGetProductInfo(string productId, out ProductInfo info)
        {
            if (_products.TryGetValue(productId, out info))
                return true;

            info = null;
            return false;
        }

        /// <summary>
        /// Lista todos os produtos carregados da Google Play.
        /// </summary>
        public IEnumerable<ProductInfo> GetAllProducts()
        {
            return _products.Values;
        }

        #endregion

        #region Compra

        /// <summary>
        /// Inicia a compra de um produto pelo ID.
        /// </summary>
        public void InitiatePurchase(string productId)
        {
            if (StoreController == null)
            {
                Debug.LogWarning("Store not initialized yet");
                return;
            }

            var product = StoreController.products.WithID(productId);
            if (product == null)
            {
                Debug.LogError($"Product with id {productId} not found in IAP catalog");
                return;
            }

            StoreController.InitiatePurchase(product);
        }

        public void Consumable_Btn_Pressed()
        {
            if (!string.IsNullOrEmpty(ncItem.id))
                InitiatePurchase(ncItem.id);
        }

        public void NonConsumable_Btn_Pressed()
        {
            if (!string.IsNullOrEmpty(ncItem.id))
                InitiatePurchase(ncItem.id);
        }

        public void Subscription_Btn_Pressed()
        {
            if (!string.IsNullOrEmpty(sItem.id))
                InitiatePurchase(sItem.id);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            var product = purchaseEvent.purchasedProduct;
            Debug.Log("Purchase Complete: " + product.definition.id);
            
            var coinPack = coinPackList.FirstOrDefault(p => p.productId == product.definition.id);
            if (coinPack != null)
            {
                AddCoins(coinPack.coinAmount);
                return PurchaseProcessingResult.Complete;
            }
            
            if (!string.IsNullOrEmpty(ncItem.id) && product.definition.id == ncItem.id)
            {
                RemoveAds();
                return PurchaseProcessingResult.Complete;
            }
            
            if (!string.IsNullOrEmpty(sItem.id) && product.definition.id == sItem.id)
            {
                ActivateElitePass();
                return PurchaseProcessingResult.Complete;
            }

            Debug.Log("Purchased product not handled: " + product.definition.id);
            return PurchaseProcessingResult.Complete;
        }

        #endregion

        #region Restauração / verificação

        private void CheckNonConsumable(string id)
        {
            if (StoreController == null || string.IsNullOrEmpty(id))
                return;

            var product = StoreController.products.WithID(id);
            if (product != null)
            {
                if (product.hasReceipt)
                {
                    RemoveAds();
                }
                else
                {
                    ShowAds();
                }
            }
        }

        private void CheckSubscription(string id)
        {
            if (StoreController == null || string.IsNullOrEmpty(id))
                return;

            var subProduct = StoreController.products.WithID(id);
            if (subProduct == null)
            {
                Debug.Log("Subscription product not found!");
                return;
            }

            try
            {
                if (subProduct.hasReceipt)
                {
                    var subManager = new SubscriptionManager(subProduct, null);
                    var info = subManager.GetSubscriptionInfo();

                    if (info.IsSubscribed() == Result.True)
                    {
                        Debug.Log("We are subscribed");
                        ActivateElitePass();
                    }
                    else
                    {
                        Debug.Log("Unsubscribed");
                        DeActivateElitePass();
                    }
                }
                else
                {
                    Debug.Log("Receipt not found!");
                }
            }
            catch (Exception)
            {
                Debug.Log("Subscription check works only on real stores (Google, Apple, Amazon, etc.).");
            }
        }

        #endregion

        #region Erros

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("IAP initialize failed: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log($"IAP initialize failed: {error} - {message}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.Log("Purchase failed: " + failureReason);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            Debug.Log("Purchase failed: " + failureDescription);
        }

        #endregion

        #region Lógica específica do jogo (coins / ads / VIP)

        private float _val;

        private void AddCoins(int num)
        {
            int coins = PlayerPrefs.GetInt("totalCoins");
            coins += num;
            PlayerPrefs.SetInt("totalCoins", coins);

            if (coinTxt != null)
                StartCoroutine(CoinShakeEffect(coins - num, coins, .5f));
        }

        private IEnumerator CoinShakeEffect(int oldValue, int newValue, float animTime)
        {
            float ct = 0;
            float nt;
            float tot = animTime;

            var anim = coinTxt != null ? coinTxt.GetComponent<Animation>() : null;
            anim?.Play("textShake");

            while (ct < tot)
            {
                ct += Time.deltaTime;
                nt = ct / tot;
                _val = Mathf.Lerp(oldValue, newValue, nt);

                if (coinTxt != null)
                    coinTxt.text = ((int)_val).ToString();

                yield return null;
            }

            anim?.Stop();
        }

        [Header("Non Consumable UI")]
        public GameObject AdsPurchasedWindow;
        public GameObject adsBanner;

        private void RemoveAds()
        {
            DisplayAds(false);
        }

        private void ShowAds()
        {
            DisplayAds(true);
        }

        private void DisplayAds(bool showAds)
        {
            if (!showAds)
            {
                if (AdsPurchasedWindow != null) AdsPurchasedWindow.SetActive(true);
                if (adsBanner != null) adsBanner.SetActive(false);
            }
            else
            {
                if (AdsPurchasedWindow != null) AdsPurchasedWindow.SetActive(false);
                if (adsBanner != null) adsBanner.SetActive(true);
            }
        }

        [Header("Subscription UI")]
        public GameObject subActivatedWindow;
        public GameObject premiumBanner;

        private void ActivateElitePass()
        {
            SetupElitePass(true);
        }

        private void DeActivateElitePass()
        {
            SetupElitePass(false);
        }

        private void SetupElitePass(bool active)
        {
            if (subActivatedWindow != null)
                subActivatedWindow.SetActive(active);

            if (premiumBanner != null)
                premiumBanner.SetActive(active);
        }

        #endregion
    }
}
