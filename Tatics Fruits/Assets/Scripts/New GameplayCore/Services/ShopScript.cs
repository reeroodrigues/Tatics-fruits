using System;
using System.Collections;
using System.Collections.Generic;
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
        public int timeDuration; //in Days
    }
    public class ShopScript : MonoBehaviour, IDetailedStoreListener
    {
        IStoreController m_StoreController;
    
        public ConsumableItem cItem;
        public NonConsumableItem ncItem;
        public SubscriptionItem sItem;
    
        public TextMeshProUGUI inp;

        public Data data;
        public Payload payload;
        public PayloadData payloadData;

        private void Start()
        {
            var coins = PlayerPrefs.GetInt("totalCoins");
            coinTxt.text = coins.ToString();
            SetupBuilder();
        }
    
        #region setup and initialize

        void SetupBuilder()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        
            builder.AddProduct(cItem.id, ProductType.Consumable);
            builder.AddProduct(ncItem.id, ProductType.NonConsumable);
            builder.AddProduct(sItem.id, ProductType.Subscription);
        
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            print("Sucess");
            m_StoreController = controller;
            CheckNonConsumable(ncItem.id);
            CheckSubscription(sItem.id);
        }
        #endregion
    
        #region button clicks
        public void Consumable_Btn_Pressed()
        {
            //AddCoins(50);
            m_StoreController.InitiatePurchase(cItem.id);
        }

        public void NonConsumable_Btn_Pressed()
        {
            //RemoveAds();
            m_StoreController.InitiatePurchase(ncItem.id);
        }

        public void Subscription_Btn_Pressed()
        {
            //ActivateElitePass();
            m_StoreController.InitiatePurchase(sItem.id);
        }
        #endregion
    
        #region main
        //processing purchase
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            //Retrieve the purchased product
            var product = purchaseEvent.purchasedProduct;
        
            print("Purchase Complete" + product.definition.id);

            if (product.definition.id == cItem.id) //consumable item is pressed
            {
                string receipt = product.receipt;
                data = JsonUtility.FromJson<Data>(receipt);
                payload = JsonUtility.FromJson<Payload>(data.Payload);
                payloadData = JsonUtility.FromJson<PayloadData>(payload.json);

                int quantity = payloadData.quantity;

                for (int i = 0; i < quantity; i++)
                {
                    AddCoins(50);
                }
            }
            else if (product.definition.id == ncItem.id)//non consumable
            {
                RemoveAds();
            }
            else if(product.definition.id == sItem.id)//subscribed
            {
                ActivateElitePass();
            }
        
            return PurchaseProcessingResult.Complete;
        }
        #endregion


        void CheckNonConsumable(string id)
        {
            if (m_StoreController != null)
            {
                var product = m_StoreController.products.WithID(id);
                if (product != null)
                {
                    if (product.hasReceipt) //purchased
                    {
                        RemoveAds();
                    }
                    else
                    {
                        ShowAds();
                    }
                }
            }
        }

        void CheckSubscription(string id)
        {
            var subProduct = m_StoreController.products.WithID(id);
            if (subProduct != null)
            {
                try
                {
                    if (subProduct.hasReceipt)
                    {
                        var subManager = new SubscriptionManager(subProduct, null);
                        var info = subManager.GetSubscriptionInfo();
                        /*print(info.GetCancelDate());
                    print(info.GetExpireDate());
                    print(info.GetFreeTrialPeriod());
                    print(info.GetIntroductoryPrice());
                    print(info.GetProductId());
                    print(info.GetPurchaseDate());
                    print(info.GetRemainingTime());
                    print(info.GetSkuDetails());
                    print(info.GetSubscriptionPeriod());
                    print(info.IsAutoRenewing());
                    print(info.IsCancelled());
                    print(info.IsExpired());
                    print(info.IsFreeTrial());
                    print(info.IsSubscribed());*/

                        if (info.IsSubscribed() == Result.True)
                        {
                            print("We are subscribed");
                            ActivateElitePass();
                        }
                        else
                        {
                            print("Un subscribed");
                            DeActivateElitePass();
                        }
                    
                    }
                    else
                    {
                        print("Receipt not found!");
                    }
                }
                catch (Exception )
                {
                    print("It only work for Google store, app store, amazon store, you are using fake store!");
                }
            }
            else
            {
                print("Product not found!");
            }
        }
    
        #region error handeling

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            print("failed" + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            print("initialize failed" + error + message);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            print("purchase failed" + failureReason);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            print("purchase failed" + failureDescription);
        }
        #endregion
    
        #region extra
        [Header("Consumable")]
        public TextMeshProUGUI coinTxt;

        void AddCoins(int num)
        {
            int coins = PlayerPrefs.GetInt("totalCoins");
            coins += num;
            PlayerPrefs.SetInt("totalCoins", coins);
            StartCoroutine(startCoinShakeEffect(coins - num, coins, .5f));
        }
        float val;

        IEnumerator startCoinShakeEffect(int oldValue, int newValue, float animTime)
        {
            float ct = 0;
            float nt;
            float tot = animTime;
            coinTxt.GetComponent<Animation>().Play("textShake");
            while (ct < tot)
            {
                ct += Time.deltaTime;
                nt = ct / tot;
                val = Mathf.Lerp(oldValue, newValue, nt);
                coinTxt.text = ((int)(val)).ToString();
                yield return null;
            }
            coinTxt.GetComponent<Animation>().Stop();
        }
        [Header("Non Consumable")]
        public GameObject AdsPurchasedWindow;
        public GameObject adsBanner;
        void RemoveAds()
        {
            DisplayAds(false);
        }
        void ShowAds()
        {
            DisplayAds(true);

        }
        void DisplayAds(bool x)
        {
            if (!x)
            {
                AdsPurchasedWindow.SetActive(true);
                adsBanner.SetActive(false);
            }
            else
            {
                AdsPurchasedWindow.SetActive(false);
                adsBanner.SetActive(true);
            }
        }

        [Header("Subscription")]
        public GameObject subActivatedWindow;
        public GameObject premiumBanner;

        void ActivateElitePass()
        {
            setupElitePass(true);
        }
        void DeActivateElitePass()
        {
            setupElitePass(false);
        }
        void setupElitePass(bool x)
        {
            if (x)// active
            {
                subActivatedWindow.SetActive(true);
                premiumBanner.SetActive(true);
            }
            else
            {
                subActivatedWindow.SetActive(false);
                premiumBanner.SetActive(false);
            }
        }

   

        #endregion

    }


    [Serializable]
    public class SkuDetails
    {
        public string productId;
        public string type;
        public string title;
        public string name;
        public string iconUrl;
        public string description;
        public string price;
        public long price_amount_micros;
        public string price_currency_code;
        public string skuDetailsToken;
    }

    [Serializable]
    public class PayloadData
    {
        public string orderId;
        public string packageName;
        public string productId;
        public long purchaseTime;
        public int purchaseState;
        public string purchaseToken;
        public int quantity;
        public bool acknowledged;
    }

    [Serializable]
    public class Payload
    {
        public string json;
        public string signature;
        public List<SkuDetails> skuDetails;
        public PayloadData payloadData;
    }

    [Serializable]
    public class Data
    {
        public string Payload;
        public string Store;
        public string TransactionID;
    }
}