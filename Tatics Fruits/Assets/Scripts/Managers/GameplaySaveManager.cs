using UnityEngine;
using Managers;

public class GameplaySaveManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool disableAutoSaveDuringGameplay = true;
    [SerializeField] private bool saveOnLevelStart = false;
    [SerializeField] private bool saveOnLevelComplete = true;

    private bool _wasAutoSaveEnabled;

    private void Start()
    {
        if (disableAutoSaveDuringGameplay && PlayerDataManager.Instance != null)
        {
            _wasAutoSaveEnabled = true;
            PlayerDataManager.Instance.EnableAutoSave(false);
            Debug.Log("[GameplaySave] Auto-save disabled during active gameplay");
        }

        if (saveOnLevelStart)
        {
            SaveHelper.SaveToLocal();
            Debug.Log("[GameplaySave] Progress saved at level start");
        }
    }

    public void OnLevelComplete(int levelIndex, int coinsEarned, int score, int stars = 3)
    {
        Debug.Log($"[GameplaySave] Level {levelIndex} completed - Coins: {coinsEarned}, Score: {score}, Stars: {stars}");
        
        SaveHelper.OnLevelComplete(levelIndex, coinsEarned, score, stars);
        
        AnalyticsManager.Instance?.TrackLevelCompleted(levelIndex, score, stars, Time.timeSinceLevelLoad, true);
        AnalyticsManager.Instance?.TrackCurrencyEarned(coinsEarned, $"level_{levelIndex}_completion", "gold");
        
        var dailyMissions = FindFirstObjectByType<DailyMissionsController>();
        if (dailyMissions != null)
        {
            dailyMissions.ReportWinLevel(levelIndex);
        }
        
        Debug.Log($"[GameplaySave] ✅ Level {levelIndex} progress saved to local and Firebase");
    }

    public void OnLevelFailed(int levelIndex, int score, string failReason = "time_up")
    {
        Debug.Log($"[GameplaySave] Level {levelIndex} failed - Score: {score}, Reason: {failReason}");
        
        AnalyticsManager.Instance?.TrackLevelFailed(levelIndex, score, Time.timeSinceLevelLoad, failReason);
    }

    public void OnCollectCoins(int amount)
    {
        if (amount <= 0) return;
        
        SaveHelper.AddCoins(amount);
        Debug.Log($"[GameplaySave] Collected {amount} coins during gameplay");
    }

    public void OnPowerUpUsed(string powerUpType, int levelIndex)
    {
        AnalyticsManager.Instance?.TrackPowerUpUsed(powerUpType, levelIndex);
        Debug.Log($"[GameplaySave] Power-up used: {powerUpType}");
    }

    private void OnDestroy()
    {
        if (disableAutoSaveDuringGameplay && _wasAutoSaveEnabled && PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.EnableAutoSave(true);
            Debug.Log("[GameplaySave] Auto-save re-enabled after gameplay");
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveHelper.SaveToLocal();
            Debug.Log("[GameplaySave] Progress saved on pause during gameplay");
        }
    }
}
