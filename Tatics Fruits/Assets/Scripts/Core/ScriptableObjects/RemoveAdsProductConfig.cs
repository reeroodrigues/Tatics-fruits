using UnityEngine;

namespace Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "RemoveAdsProductConfig", menuName = "Game/IAP/Remove Ads Config")]
    public class RemoveAdsProductConfig : ScriptableObject
    {
        [Header("Product Configuration")] 
        public string googlePlayProductId = "";
        public string appleStoreProductId = "";

        [Header("Display Information")]
        public string productName = "Remove Ads";
        public string description = "Remove all ads from the game";
        public float priceUSD = 2.99f;

        [Header("Settings")] 
        public bool isNonConsumable = true;
    }
}