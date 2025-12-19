using System.Collections;
using Core.Services;
using Gameplay.Controllers;
using UnityEngine;

namespace Managers
{
    public class SaveSystemConfigurator : MonoBehaviour
    {
        [Header("Auto-Save Settings")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveIntervalSeconds = 120f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Start()
        {
            StartCoroutine(InitializeSaveSystem());
        }

        private IEnumerator InitializeSaveSystem()
        {
            yield return new WaitForSeconds(0.5f);

            ConfigureAutoSave();
            ValidateSaveSystem();
            
            if (showDebugLogs)
            {
                LogSaveSystemStatus();
            }
        }

        private void ConfigureAutoSave()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SetAutoSaveInterval(autoSaveIntervalSeconds);
                PlayerDataManager.Instance.EnableAutoSave(enableAutoSave);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[SaveSystemConfig] ✅ Auto-save configured - Enabled: {enableAutoSave}, Interval: {autoSaveIntervalSeconds}s");
                }
            }
            else
            {
                Debug.LogError("[SaveSystemConfig] ❌ PlayerDataManager not found!");
            }
        }

        private void ValidateSaveSystem()
        {
            bool allSystemsReady = true;

            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[SaveSystemConfig] ❌ PlayerDataManager is missing!");
                allSystemsReady = false;
            }

            var dataSaver = FindFirstObjectByType<DataSaver>();
            if (dataSaver == null)
            {
                Debug.LogError("[SaveSystemConfig] ❌ DataSaver is missing!");
                allSystemsReady = false;
            }

            var lifecycleManager = FindFirstObjectByType<ApplicationLifecycleManager>();
            if (lifecycleManager == null)
            {
                Debug.LogWarning("[SaveSystemConfig] ⚠️ ApplicationLifecycleManager not found - save on quit/pause will not work!");
                allSystemsReady = false;
            }

            var firebaseSync = FindFirstObjectByType<FirebaseProfileSyncService>();
            if (firebaseSync == null)
            {
                Debug.LogWarning("[SaveSystemConfig] ⚠️ FirebaseProfileSyncService not found - cloud sync disabled!");
            }

            var profileController = FindFirstObjectByType<PlayerProfileController>();
            if (profileController == null)
            {
                Debug.LogWarning("[SaveSystemConfig] ⚠️ PlayerProfileController not found!");
            }

            if (allSystemsReady && showDebugLogs)
            {
                Debug.Log("[SaveSystemConfig] ✅ All core save systems are ready!");
            }
        }

        private void LogSaveSystemStatus()
        {
            Debug.Log("========== SAVE SYSTEM STATUS ==========");
            Debug.Log($"PlayerDataManager: {(PlayerDataManager.Instance != null ? "✅ Active" : "❌ Missing")}");
            Debug.Log($"DataSaver: {(FindFirstObjectByType<DataSaver>() != null ? "✅ Active" : "❌ Missing")}");
            Debug.Log($"ApplicationLifecycleManager: {(FindFirstObjectByType<ApplicationLifecycleManager>() != null ? "✅ Active" : "⚠️ Not Found")}");
            Debug.Log($"FirebaseProfileSyncService: {(FindFirstObjectByType<FirebaseProfileSyncService>() != null ? "✅ Active" : "⚠️ Not Found")}");
            Debug.Log($"PlayerProfileController: {(FindFirstObjectByType<PlayerProfileController>() != null ? "✅ Active" : "⚠️ Not Found")}");
            Debug.Log($"Auto-Save: {(enableAutoSave ? $"✅ Enabled ({autoSaveIntervalSeconds}s)" : "❌ Disabled")}");
            Debug.Log("========================================");
        }

        [ContextMenu("Test Save Now")]
        public void TestSaveNow()
        {
            Debug.Log("[SaveSystemConfig] Testing manual save...");
            
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SavePlayerData();
            }

            var profileController = FindFirstObjectByType<PlayerProfileController>();
            if (profileController != null)
            {
                profileController.SaveProfile();
            }

            Debug.Log("[SaveSystemConfig] ✅ Manual save completed!");
        }

        [ContextMenu("Force Sync to Firebase")]
        public void ForceSyncToFirebase()
        {
            Debug.Log("[SaveSystemConfig] Force syncing to Firebase...");
            
            var firebaseSync = FindFirstObjectByType<FirebaseProfileSyncService>();
            if (firebaseSync != null)
            {
                firebaseSync.SaveData();
                Debug.Log("[SaveSystemConfig] ✅ Firebase sync triggered!");
            }
            else
            {
                Debug.LogError("[SaveSystemConfig] ❌ FirebaseProfileSyncService not found!");
            }
        }
    }
}
