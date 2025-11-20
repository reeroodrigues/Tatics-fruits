using UnityEngine;

namespace New_GameplayCore
{
    [System.Serializable]
    public struct DeckEntrySummary { public CardTypeSo type; public int quantity; }

    [CreateAssetMenu(menuName = "Create DeckConfigSO", fileName = "DeckConfigSO", order = 0)]
    public class DeckConfigSo : ScriptableObject
    {
        public DeckEntrySummary[] entries;
    }
}