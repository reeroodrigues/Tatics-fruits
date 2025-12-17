using System;
using UnityEngine;

namespace Managers
{
    public class PlayerDataManager : MonoBehaviour
    {
        private static PlayerDataManager _instance;
        public static PlayerDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PlayerDataManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PlayerDataManager");
                        _instance = go.AddComponent<PlayerDataManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [SerializeField] private DataSaver dataSaver;

        private const string AUTO_SAVE_KEY = "autoSaveEnabled";
        private float _autoSaveInterval = 60f;
        private float _timeSinceLastSave;
        private bool _autoSaveEnabled = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _autoSaveEnabled = PlayerPrefs.GetInt(AUTO_SAVE_KEY, 1) == 1;
        }

        private void Start()
        {
            if (dataSaver == null)
            {
                dataSaver = FindFirstObjectByType<DataSaver>();
                if (dataSaver == null)
                {
                    Debug.LogError("[PlayerDataManager] DataSaver not found in scene!");
                }
            }
        }

        private void Update()
        {
            if (_autoSaveEnabled && dataSaver != null)
            {
                _timeSinceLastSave += Time.deltaTime;
                if (_timeSinceLastSave >= _autoSaveInterval)
                {
                    SavePlayerData();
                    _timeSinceLastSave = 0f;
                }
            }
        }

        public void SetDataSaver(DataSaver saver)
        {
            dataSaver = saver;
        }

        public void SetAutoSaveInterval(float intervalInSeconds)
        {
            _autoSaveInterval = Mathf.Max(10f, intervalInSeconds);
        }

        public void EnableAutoSave(bool enable)
        {
            _autoSaveEnabled = enable;
            PlayerPrefs.SetInt(AUTO_SAVE_KEY, enable ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SavePlayerData()
        {
            if (dataSaver == null)
            {
                Debug.LogWarning("[PlayerDataManager] DataSaver is null. Cannot save data.");
                return;
            }
            dataSaver.SaveData();
        }

        public void LoadPlayerData()
        {
            if (dataSaver == null)
            {
                Debug.LogWarning("[PlayerDataManager] DataSaver is null. Cannot load data.");
                return;
            }
            dataSaver.LoadData();
        }

        public string GetPlayerName()
        {
            return dataSaver?.dataToSave?.userName ?? "Guest";
        }

        public void SetPlayerName(string playerName)
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.userName = playerName;
            }
        }

        public int GetUniquePlayerId()
        {
            return dataSaver?.dataToSave?.uniquePlayerId ?? 0;
        }

        public void SetUniquePlayerId(int playerId)
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.uniquePlayerId = playerId;
            }
        }

        public int GetPlayerLevel()
        {
            return dataSaver?.dataToSave?.crrLevel ?? 1;
        }

        public void SetPlayerLevel(int level)
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.crrLevel = Mathf.Max(1, level);
            }
        }

        public void IncrementPlayerLevel()
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.crrLevel++;
            }
        }

        public int GetCoins()
        {
            return dataSaver?.dataToSave?.totalCoins ?? 0;
        }

        public void SetCoins(int coins)
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.totalCoins = Mathf.Max(0, coins);
            }
        }

        public void AddCoins(int amount)
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.totalCoins += amount;
                dataSaver.dataToSave.totalCoins = Mathf.Max(0, dataSaver.dataToSave.totalCoins);
            }
        }

        public bool SpendCoins(int amount)
        {
            if (dataSaver?.dataToSave == null) return false;

            if (dataSaver.dataToSave.totalCoins >= amount)
            {
                dataSaver.dataToSave.totalCoins -= amount;
                return true;
            }
            return false;
        }

        public int GetUnlockedAvatarsCount()
        {
            return dataSaver?.dataToSave?.unlockedAvatar?.Count ?? 0;
        }

        public bool IsAvatarUnlocked(int avatarId)
        {
            return dataSaver?.dataToSave?.unlockedAvatar?.Contains(avatarId) ?? false;
        }

        public void UnlockAvatar(int avatarId)
        {
            if (dataSaver?.dataToSave?.unlockedAvatar != null)
            {
                if (!dataSaver.dataToSave.unlockedAvatar.Contains(avatarId))
                {
                    dataSaver.dataToSave.unlockedAvatar.Add(avatarId);
                }
            }
        }

        public void LockAvatar(int avatarId)
        {
            if (dataSaver?.dataToSave?.unlockedAvatar != null)
            {
                dataSaver.dataToSave.unlockedAvatar.Remove(avatarId);
            }
        }

        public bool GetRemoveAds()
        {
            return dataSaver?.dataToSave?.removeAds ?? false;
        }

        public void SetRemoveAds(bool removeAds)
        {
            if (dataSaver?.dataToSave != null)
            {
                dataSaver.dataToSave.removeAds = removeAds;
            }
        }

        public DataToSave GetFullPlayerData()
        {
            return dataSaver?.dataToSave;
        }
    }
}
