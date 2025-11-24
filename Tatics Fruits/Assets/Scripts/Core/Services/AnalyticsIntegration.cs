using Managers;
using Unity.Services.Analytics;

namespace Core.Services
{
    public static class AnalyticsIntegration
    {
        public static void TrackSceneTransition(string fromScene, string toScene)
        {
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackSceneTransition(fromScene, toScene);
            }
        }

        public static void TrackCombo(int combo, int scoreEarned)
        {
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.TrackCombo(combo, scoreEarned);
            }
        }
    }
}