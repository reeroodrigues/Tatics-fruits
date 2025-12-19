using UnityEngine;
using Core.Services;
using Gameplay.Controllers;

namespace Managers
{
    public class SaveSystemInitializer : MonoBehaviour
    {
        [Header("Auto-Save Configuration")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 120f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            InitializeAutoSave();
            VerifySaveSystemComponents();
        }

        private void InitializeAutoSave()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.EnableAutoSave(enableAutoSave);
                PlayerDataManager.Instance.SetAutoSaveInterval(autoSaveInterval);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[SaveSystem] Auto-save {(enableAutoSave ? "enabled" : "disabled")} " +
                              $"(Interval: {autoSaveInterval}s)");
                }
            }
            else
            {
                Debug.LogWarning("[SaveSystem] PlayerDataManager not found!");
            }
        }

        private void VerifySaveSystemComponents()
        {
            if (!showDebugLogs) return;
            
            Debug.Log("========== SAVE SYSTEM INITIALIZATION ==========");
            
            bool hasPlayerDataManager = PlayerDataManager.Instance != null;
            bool hasProfileController = FindFirstObjectByType<PlayerProfileController>() != null;
            bool hasFirebaseSync = FindFirstObjectByType<FirebaseProfileSyncService>() != null;
            bool hasDataSaver = FindFirstObjectByType<DataSaver>() != null;
            bool hasLifecycleManager = FindFirstObjectByType<ApplicationLifecycleManager>() != null;
            
            Debug.Log($"PlayerDataManager: {(hasPlayerDataManager ? "✅" : "❌")}");
            Debug.Log($"PlayerProfileController: {(hasProfileController ? "✅" : "❌")}");
            Debug.Log($"FirebaseProfileSyncService: {(hasFirebaseSync ? "✅" : "❌")}");
            Debug.Log($"DataSaver: {(hasDataSaver ? "✅" : "❌")}");
            Debug.Log($"ApplicationLifecycleManager: {(hasLifecycleManager ? "✅" : "❌")}");
            
            Debug.Log("===============================================");
        }

        [ContextMenu("Force Save All")]
        public void ForceSaveAll()
        {
            SaveHelper.SaveAll();
            Debug.Log("[SaveSystem] ✅ Manual save triggered");
        }

        [ContextMenu("Check Save System Status")]
        public void CheckStatus()
        {
            SaveHelper.LogSaveSystemStatus();
        }
    }
}
