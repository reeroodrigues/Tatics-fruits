using Managers;
using UnityEngine;

namespace Core.Services
{
    public class AnalyticsIntegrationExample : MonoBehaviour
    {
        private void TrackStoreExample()
        {
            AnalyticsManager.Instance?.TrackStoreOpened();
        }

        private void TrackPurchaseExample()
        {
            AnalyticsManager.Instance?.TrackItemPurchased(
                itemId: "powerup_bomb",
                itemType: "powerup",
                price: 100,
                currency: "gold"
            );
        }

        private void TrackCurrencyExample()
        {
            AnalyticsManager.Instance?.TrackCurrencyEarned(
                amount: 50,
                source: "level_complete",
                currency: "gold"
            );
        }

        private void TrackAdExample()
        {
            AnalyticsManager.Instance?.TrackAdWatched(
                adType: "rewarded",
                placement: "double_coins",
                completed: true
            );
        }

        private void TrackMenuExample()
        {
            AnalyticsManager.Instance?.TrackMenuOpened("settings");
        }

        private void TrackSettingsExample()
        {
            AnalyticsManager.Instance?.TrackSettingsChanged("sound_enabled", true);
        }
    }
}
