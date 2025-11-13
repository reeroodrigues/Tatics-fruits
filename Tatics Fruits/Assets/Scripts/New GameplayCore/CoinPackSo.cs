using UnityEngine;

public enum CoinPackSize
{
    Small,
    Medium,
    Large
}

namespace New_GameplayCore
{
    [CreateAssetMenu(fileName = "CoinPack",menuName = "Store/CoinPack")]
    public class CoinPackSo : ScriptableObject
    {
        [Header("Product Info(IAP ID)")]
        public string productId;

        [Header("Display")]
        public string displayName;
        public Sprite icon;

        [Header("Coins")] 
        public int coinAmount;
        
        [Header("Category")]
        public CoinPackSize size;

        [Header("Preview Price(visual only)")]
        public string priceText;
    }
    
}