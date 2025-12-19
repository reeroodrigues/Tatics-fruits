using Gameplay.Controllers;
using UnityEngine;

namespace Managers
{
    public class ApplicationLifecycleManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool saveOnQuit = true;
        [SerializeField] private bool saveOnPause = true;
        [SerializeField] private bool saveOnFocusLost = true;
        [SerializeField] private float saveCooldown = 3f;

        private float lastSaveTime;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ApplicationLifecycle] Manager initialized");
        }

        private void OnApplicationQuit()
        {
            if (saveOnQuit)
            {
                Debug.Log("[ApplicationLifecycle] Application quitting - saving player data");
                SaveGameState();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (saveOnPause && isPaused)
            {
                if (Time.time - lastSaveTime > saveCooldown)
                {
                    Debug.Log("[ApplicationLifecycle] Application paused - saving player data");
                    SaveGameState();
                    lastSaveTime = Time.time;
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (saveOnFocusLost && !hasFocus)
            {
                if (Time.time - lastSaveTime > saveCooldown)
                {
                    Debug.Log("[ApplicationLifecycle] Application lost focus - saving player data");
                    SaveGameState();
                    lastSaveTime = Time.time;
                }
            }
        }

        private void SaveGameState()
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SavePlayerData();
            }

            var profileController = FindFirstObjectByType<PlayerProfileController>();
            if (profileController != null)
            {
                profileController.SaveProfile();
            }

            Debug.Log("[ApplicationLifecycle] Game state saved successfully");
        }
    }
}
