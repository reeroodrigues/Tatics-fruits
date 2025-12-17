using Core.Services;
using Managers;
using UnityEngine;

public static class SaveHelper
{
    public static void SaveAll()
    {
        SaveToLocal();
        SyncToFirebase();
        Debug.Log("[SaveHelper] ✅ All save systems triggered");
    }

    public static void SaveToLocal()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SavePlayerData();
        }

        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.SaveProfile();
        }
    }

    public static void SyncToFirebase()
    {
        var firebaseSync = Object.FindFirstObjectByType<FirebaseProfileSyncService>();
        if (firebaseSync != null)
        {
            firebaseSync.SaveData();
        }
    }

    public static void OnLevelComplete(int levelIndex, int coinsEarned, int score, int stars = 3)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.OnLevelComplete(levelIndex, coinsEarned, score, stars);
        }
        else
        {
            Debug.LogWarning("[SaveHelper] PlayerProfileController not found!");
        }
    }

    public static void OnPurchaseAvatar(int avatarId, int price)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.PurchaseAvatar(avatarId, price);
        }
        else
        {
            Debug.LogWarning("[SaveHelper] PlayerProfileController not found!");
        }
    }

    public static void OnPurchaseCard(string cardId, int price)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.TryPurchaseCard(cardId, price);
        }
        else
        {
            Debug.LogWarning("[SaveHelper] PlayerProfileController not found!");
        }
    }

    public static void OnSettingsChanged(bool music, bool sfx, bool vfx, string language)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.SetMusicEnabled(music);
            profileController.SetSfxEnabled(sfx);
            profileController.SetVfxEnabled(vfx);
            profileController.SetLanguage(language);
        }
        else
        {
            Debug.LogWarning("[SaveHelper] PlayerProfileController not found!");
        }
    }

    public static void OnRemoveAdsPurchased()
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.SetRemoveAds(true);
        }
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.SetRemoveAds(true);
            PlayerDataManager.Instance.SavePlayerData();
        }
        
        Debug.Log("[SaveHelper] ✅ Remove Ads purchased and saved!");
    }

    public static void AddCoins(int amount)
    {
        if (amount <= 0) return;
        
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.AddGold(amount);
        }
        
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.AddCoins(amount);
        }
    }

    public static bool TrySpendCoins(int amount)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            return profileController.TrySpendGold(amount);
        }

        if (PlayerDataManager.Instance != null)
        {
            return PlayerDataManager.Instance.SpendCoins(amount);
        }

        return false;
    }

    public static int GetCoins()
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            return profileController.Data.gold;
        }

        if (PlayerDataManager.Instance != null)
        {
            return PlayerDataManager.Instance.GetCoins();
        }

        return 0;
    }

    public static void UnlockAvatar(int avatarId)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.UnlockAvatar(avatarId);
        }

        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.UnlockAvatar(avatarId);
            PlayerDataManager.Instance.SavePlayerData();
        }
    }

    public static void OnDailyRewardClaimed(string dayKey, int rewardCoins)
    {
        AddCoins(rewardCoins);
        
        var firebaseSync = Object.FindFirstObjectByType<FirebaseProfileSyncService>();
        if (firebaseSync != null)
        {
            firebaseSync.UpdateDailyLogin(dayKey);
        }
        
        Debug.Log($"[SaveHelper] ✅ Daily reward claimed - Day: {dayKey}, Coins: {rewardCoins}");
    }

    public static void OnDeckChanged(System.Collections.Generic.List<string> equippedDeck)
    {
        var profileController = Object.FindFirstObjectByType<PlayerProfileController>();
        if (profileController != null)
        {
            profileController.SaveProfile();
        }
        
        Debug.Log($"[SaveHelper] ✅ Deck saved with {equippedDeck.Count} cards");
    }

    public static void LogSaveSystemStatus()
    {
        Debug.Log("========== SAVE SYSTEM STATUS ==========");
        Debug.Log($"PlayerDataManager: {(PlayerDataManager.Instance != null ? "✅" : "❌")}");
        Debug.Log($"PlayerProfileController: {(Object.FindFirstObjectByType<PlayerProfileController>() != null ? "✅" : "❌")}");
        Debug.Log($"FirebaseSync: {(Object.FindFirstObjectByType<FirebaseProfileSyncService>() != null ? "✅" : "❌")}");
        Debug.Log($"DataSaver: {(Object.FindFirstObjectByType<DataSaver>() != null ? "✅" : "❌")}");
        Debug.Log($"Coins: {GetCoins()}");
        Debug.Log("========================================");
    }
}
