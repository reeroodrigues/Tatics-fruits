using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.SaveSystem;
using Firebase.Database;
using UnityEngine;

namespace Core.Services
{
    [Serializable]
    public class DataToSave
    {
        public string userName;
        public int totalCoins;
        public int crrLevel;
        public int highScore;
        public long lastUpdatedTicks;
        public bool isVip;
        public long vipExpirationTicks;
        public List<string> ownedCards = new List<string>();
        public List<string> equippedDeck = new List<string>();
        public List<int> unlockedAvatar =  new List<int>{0};
        public List<int> purchasedAvatar = new List<int>();
        public Dictionary<string, int> bestScores =  new Dictionary<string, int>();

        public bool musicOn = true;
        public bool sfxOn = true;
        public bool vfxOn = true;
        public string language = "pt_BR";
        public string dailyDayKey;
        public string lastLoginDayKey;
    }
    public class FirebaseProfileSyncService : MonoBehaviour
    {
        [Header("Player Data")]
        [SerializeField] private string userId;
        [SerializeField] public DataToSave dataToSave = new DataToSave();

        private DatabaseReference _databaseReference;
        private string _localFilePath;

        public event Action<DataToSave> OnDataLoaded;
        public event Action OnDataNotFound;
        public event Action<Exception> OnLoadFailed;

        private void Awake()
        {
            _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            
            _localFilePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        }

        /// <summary>
        /// Salva dados no local (JSON) e tenta salvar no Firebase.
        /// </summary>
        public void SaveData()
        {
            if (!ValidateUserId()) return;

            if (dataToSave == null)
                dataToSave = new DataToSave();
            
            dataToSave.lastUpdatedTicks = DateTime.UtcNow.Ticks;
            
            SaveLocal();
            
            var json = JsonUtility.ToJson(dataToSave);
            _databaseReference
                .Child("users")
                .Child(userId)
                .SetRawJsonValueAsync(json)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Debug.LogError($"[DataSaver] Erro ao salvar dados no Firebase: {task.Exception}");
                    }
                    else if (task.IsCompletedSuccessfully)
                    {
                        Debug.Log("[DataSaver] Dados salvos com sucesso no Firebase.");
                    }
                });
        }

        public void SetUserId(string uid)
        {
            userId = uid;
        }

        /// <summary>
        /// Carrega dados do Firebase + Local, resolve conflito e sincroniza.
        /// </summary>
        public void LoadData()
        {
            if (!ValidateUserId()) return;
            StartCoroutine(LoadDataCoroutine());
        }

        private IEnumerator LoadDataCoroutine()
        {
            var serverDataTask = _databaseReference.Child("users").Child(userId).GetValueAsync();

            var localData = LoadLocal();

            yield return new WaitUntil(() => serverDataTask.IsCompleted);

            DataToSave cloudData = null;

            if (serverDataTask.IsFaulted)
            {
                OnLoadFailed?.Invoke(serverDataTask.Exception);
            }
            else
            {
                var snapshot = serverDataTask.Result;

                if (snapshot != null && snapshot.Exists)
                {
                    var jsonData = snapshot.GetRawJsonValue();

                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        cloudData = JsonUtility.FromJson<DataToSave>(jsonData);
                    }
                    else
                    {
                    }
                }
            }

            dataToSave = ResolveDataConflict(localData, cloudData);

            if (dataToSave != null)
            {

                if (dataToSave.lastUpdatedTicks == 0)
                    dataToSave.lastUpdatedTicks = DateTime.UtcNow.Ticks;

                SaveLocal();
                var json = JsonUtility.ToJson(dataToSave);
                _databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json);

                OnDataLoaded?.Invoke(dataToSave);

                var profileController = FindObjectOfType<PlayerProfileController>();
                if (profileController != null && profileController.Data != null)
                {
                    profileController.Data.playerName = dataToSave.userName;
                    profileController.Data.gold = dataToSave.totalCoins;
                    profileController.Data.currentLevelIndex = dataToSave.crrLevel;
                    profileController.Data.highestLevelUnlocked = dataToSave.highScore;

                    if (dataToSave.ownedCards != null)
                        profileController.Data.ownedCards = new List<string>(dataToSave.ownedCards);

                    if (dataToSave.equippedDeck != null)
                        profileController.Data.equippedDeck = new List<string>(dataToSave.equippedDeck);

                    if (dataToSave.unlockedAvatar != null)
                        profileController.Data.unlockedAvatars = new List<int>(dataToSave.unlockedAvatar);

                    if (dataToSave.purchasedAvatar != null)
                        profileController.Data.purchasedAvatars = new List<int>(dataToSave.purchasedAvatar);

                    if (dataToSave.bestScores != null)
                        profileController.Data.BestScores = new Dictionary<string, int>(dataToSave.bestScores);

                    profileController.Data.musicOn = dataToSave.musicOn;
                    profileController.Data.sfxOn = dataToSave.sfxOn;
                    profileController.Data.vfxOn = dataToSave.vfxOn;
                    profileController.Data.language = dataToSave.language;

                    if (profileController.Data.daily != null)
                    {
                        profileController.Data.daily.dayKey = dataToSave.dailyDayKey ?? "";
                        if (profileController.Data.daily.login != null)
                            profileController.Data.daily.login.lastClaimDayKey = dataToSave.lastLoginDayKey ?? "";
                    }

                    profileController.SaveProfile();
                }
            }
        }

        #region Local JSON

        private void SaveLocal()
        {
            try
            {
                var json = JsonUtility.ToJson(dataToSave);
                File.WriteAllText(_localFilePath, json);
            }
            catch (Exception e)
            {
            }
        }

        private DataToSave LoadLocal()
        {
            try
            {
                if (!File.Exists(_localFilePath))
                {
                    return null;
                }

                var json = File.ReadAllText(_localFilePath);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                var data = JsonUtility.FromJson<DataToSave>(json);
                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion

        #region Helpers

        private DataToSave ResolveDataConflict(DataToSave local, DataToSave cloud)
        {
            if (local == null && cloud == null)
                return null;
            
            if (local != null && cloud == null)
            {
                return local;
            }

            if (local == null && cloud != null)
            {
                return cloud;
            }
            
            if (cloud.lastUpdatedTicks > local.lastUpdatedTicks)
            {
                return cloud;
            }
            else
            {
                return local;
            }
        }

        private DataToSave CreateNewDefaultData()
        {
            return new DataToSave()
            {
                userName = "Guest",
                totalCoins = 0,
                crrLevel = 1,
                highScore = 0,
                lastUpdatedTicks = DateTime.Now.Ticks,
                isVip = false,
                vipExpirationTicks = 0,
                ownedCards = new List<string>(),
                equippedDeck = new List<string>(),
                unlockedAvatar = new List<int> { 0 },
                purchasedAvatar = new List<int>(),
                bestScores = new Dictionary<string, int>(),
                musicOn = true,
                sfxOn = true,
                vfxOn = true,
                language = "pt_BR",
                dailyDayKey = "",
                lastLoginDayKey = ""
            };
        }

        private bool ValidateUserId()
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            return true;
        }

        #endregion
        
        private PlayerProfileData _data;

        public PlayerProfileData Data => _data;

        public void Load()
        {
            _data = SaveManager.Instance.Load<PlayerProfileData>();
        }
        
        public void Save()
        {
            SaveManager.Instance.Save(_data);
        }

        public void MarkDirty()
        {
            SaveManager.Instance.MarkDirty<PlayerProfileData>();
        }

        public void AddGold(int amount)
        {
            _data.gold += amount;
            if (_data.gold < 0)
                _data.gold = 0;
            Save();
        }

        public void SetLevel(int index, bool completed = false)
        {
            _data.currentLevelIndex = index;
            if (completed && index >= _data.highestLevelUnlocked)
                _data.highestLevelUnlocked = index + 1;
            Save();
        }

        public void SetSettings(bool music, bool sfx, bool vfx, string lang)
        {
            _data.musicOn = music;
            _data.sfxOn = sfx;
            _data.vfxOn = vfx;
            _data.language = lang;
            Save();
        }

        public void UpdateDailyLogin(string todayKey)
        {
            if (_data.daily.login.lastClaimDayKey != todayKey)
            {
                _data.daily.login.lastClaimDayKey = todayKey;
                Save();
            }
        }

        public void UpdateDailyMissions(List<DailyMissionState> missions)
        {
            _data.daily.missions = missions;
            Save();
        }

        public void RegisterBestScore(string levelId, int score)
        {
            if(string.IsNullOrEmpty(levelId))
                return;
            
            if(!_data.BestScores.ContainsKey(levelId))
                _data.BestScores[levelId] = score;
            else if(score > _data.BestScores[levelId])
                _data.BestScores[levelId] = score;

            Save();
        }

        public int GetBestScore(string levelId)
        {
            if(string.IsNullOrEmpty(levelId))
                return 0;
            
            return _data.BestScores.TryGetValue(levelId, out int best) ? best : 0;
        }

        public void SetVipStatus(bool isVip, int durationDays = 30)
        {
            dataToSave.isVip = isVip;

            if (isVip)
            {
                dataToSave.vipExpirationTicks = DateTime.UtcNow.AddDays(durationDays).Ticks;
            }
            else
            {
                dataToSave.vipExpirationTicks = 0;
            }
            
            SaveData();
        }

        public bool IsVipActive()
        {
            if(!dataToSave.isVip)
                return false;

            if (dataToSave.vipExpirationTicks == 0)
                return true;
            
            var currentTicks = DateTime.Now.Ticks;
            var isActive = currentTicks < dataToSave.vipExpirationTicks;

            if (!isActive)
            {
                dataToSave.isVip = false;
                SaveData();
            }
            return isActive;
        }

        public void UnlockCard(string cardId)
        {
            if (!dataToSave.ownedCards.Contains(cardId))
            {
                dataToSave.ownedCards.Add(cardId);
                SaveData();
            }
        }

        public void UnlockAvatar(int avatarId)
        {
            if (!dataToSave.unlockedAvatar.Contains(avatarId))
            {
                dataToSave.unlockedAvatar.Add(avatarId);
                SaveData();
            }
        }
    }
}