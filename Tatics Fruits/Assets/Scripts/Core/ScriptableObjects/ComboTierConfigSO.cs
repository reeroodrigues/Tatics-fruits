using UnityEngine;

namespace Core.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Gameplay/Combo Tier Config", fileName = "ComboTierConfig", order = 1)]
    public class ComboTierConfigSo : ScriptableObject
    {
        [System.Serializable]
        public class ComboTier
        {
            public string tierName;
            public int minComboCount;
            public Color tierColor = Color.white;
            public float scoreMultiplier = 1f;
            public int timeBonusExtra = 0;
            public GameObject tierVFXPrefab;
        }

        [Header("Combo Tier")] public ComboTier[] tiers = new ComboTier[]
        {
            new ComboTier{ tierName =  "Combo", minComboCount = 1,  tierColor = Color.white, scoreMultiplier = 1.0f},
            new ComboTier{ tierName = "Good!", minComboCount = 3, tierColor = Color.cyan, scoreMultiplier = 1.5f, timeBonusExtra = 1},
            new ComboTier { tierName = "Great!", minComboCount = 5,tierColor = Color.yellow, scoreMultiplier = 2.0f, timeBonusExtra = 2},
            new ComboTier { tierName = "Amazing!", minComboCount = 8,tierColor = Color.magenta, scoreMultiplier = 3.0f, timeBonusExtra = 3},
            new ComboTier { tierName = "LEGENDARY!", minComboCount = 12,tierColor = Color.red, scoreMultiplier = 5.0f, timeBonusExtra = 5},
        };

        public ComboTier GetTierForCombo(int comboCount)
        {
            var result = tiers[0];

            for (int i = tiers.Length -1; i >= 0; i--)
            {
                if (comboCount >= tiers[i].minComboCount)
                {
                    result = tiers[i];
                    break;
                }
            }
            return result;
        }
    }
}