using System;
using UnityEngine;
using UnityEngine.Purchasing;
using Core.ScriptableObjects;

namespace Managers
{
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        public static IAPManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private RemoveAdsProductConfig removeAdsConfig;
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;

        private IStoreController _storeController;
        private IExtensionProvider _storeExtensionProvider;
        private bool _isInitialized = false;

        public event Action OnPurchaseSuccess;
        public event Action<string> OnPurchaseFailed;
        public event Action OnInitializationComplete;
        public event Action<string> OnInitializationFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (removeAdsConfig == null)
                return;

            InitializeIAP();
        }

        private void InitializeIAP()
        {
            if (_isInitialized)
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            var productId = GetPlatformProductId();
            
            builder.AddProduct(
                productId,
                removeAdsConfig.isNonConsumable ? ProductType.NonConsumable : ProductType.Consumable
            );

            UnityPurchasing.Initialize(this, builder);
        }

        private string GetPlatformProductId()
        {
#if UNITY_ANDROID
            return removeAdsConfig.googlePlayProductId;
#elif UNITY_IOS
            return removeAdsConfig.appleAppStoreProductId;
#else
            return removeAdsConfig.googlePlayProductId;
#endif
        }

        public void PurchaseRemoveAds()
        {
            if (!_isInitialized)
            {
                OnPurchaseFailed?.Invoke("IAP system not ready");
                return;
            }

            if (_storeController == null)
            {
                OnPurchaseFailed?.Invoke("Store not available");
                return;
            }

            var productId = GetPlatformProductId();
            var product = _storeController.products.WithID(productId);

            if (product == null)
            {
                OnPurchaseFailed?.Invoke("Product not found");
                return;
            }

            if (!product.availableToPurchase)
            {
                OnPurchaseFailed?.Invoke("Product not available");
                return;
            }
            
            _storeController.InitiatePurchase(product);
        }

        public void RestorePurchases()
        {
#if UNITY_IOS
            if (_storeExtensionProvider == null)
            {
                Debug.LogError("[IAPManager] Cannot restore - Extension provider is null");
                return;
            }
#else
            if (RemoveAdsManager.Instance != null)
            {
                RemoveAdsManager.Instance.RestorePurchase();
            }
#endif
        }

        public bool IsInitialized()
        {
            return _isInitialized;
        }

        public string GetProductPrice()
        {
            if (!_isInitialized || _storeController == null)
            {
                return $"${removeAdsConfig.priceUSD:F2}";
            }

            var productId = GetPlatformProductId();
            var product = _storeController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                return product.metadata.localizedPriceString;
            }

            return $"${removeAdsConfig.priceUSD:F2}";
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _storeExtensionProvider = extensions;
            _isInitialized = true;

            OnInitializationComplete?.Invoke();
            CheckPendingPurchases();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnInitializationFailed?.Invoke(error.ToString());
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            OnInitializationFailed?.Invoke($"{error}: {message}");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = GetPlatformProductId();
            
            if (string.Equals(args.purchasedProduct.definition.id, productId, StringComparison.Ordinal))
            {
                UnlockRemoveAds();
                OnPurchaseSuccess?.Invoke();
                return PurchaseProcessingResult.Complete;
            }

            Debug.LogWarning($"[IAPManager] Unrecognized product: {args.purchasedProduct.definition.id}");
            return PurchaseProcessingResult.Complete;
        }

        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            
            OnPurchaseFailed?.Invoke($"{failureReason}");
        }

        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            OnPurchaseFailed?.Invoke($"{failureDescription.reason}: {failureDescription.message}");
        }

        private void UnlockRemoveAds()
        {
            if (RemoveAdsManager.Instance != null)
            {
                RemoveAdsManager.Instance.UnlockRemoveAds();
            }
        }

        private void CheckPendingPurchases()
        {
            if (_storeController == null) return;

            var productId = GetPlatformProductId();
            var product = _storeController.products.WithID(productId);

            if (product != null && product.hasReceipt)
            {
                UnlockRemoveAds();
            }
        }
    }
}
