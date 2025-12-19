using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Core.SaveSystem;
using Core.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Controllers
{
    public class PlayerProfileController : MonoBehaviour
    {
        [Header("UI / Perfil")]
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private List<AvatarConfig> allAvatars;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private Button closeAvatarPanelButton;

        [Header("UI / Economia")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private GameObject goldHudRoot;
        [SerializeField] private bool goldHutVisibleByDefault = false;
    
        [Header("Deck")]
        [SerializeField] private int deckLimit = 5;

        private int _goldHudRefCount = 0;
        private const string NameRegex = @"^[A-Za-zÀ-ÖØ-öø-ÿ\s]+$";
        public bool IsLoaded { get; private set; }
        public event Action OnProfileLoaded;
        public event Action<int> OnGoldChanged;
        public PlayerProfileData Data { get; private set; } = new PlayerProfileData();

        private void Awake()
        {
            Data = SaveManager.Instance.Load<PlayerProfileData>();
            MigrateFromPlayerPrefsIfNeeded();
        
            IsLoaded = true;
            OnProfileLoaded?.Invoke();

            _goldHudRefCount = goldHutVisibleByDefault ? 1 : 0;
            ApplyGoldHudVisibility();
        }

        private void Start()
        {
            ApplyProfileUI();
            UpdateGoldUI();
            UpdateLevelUI();
            UpdateAvatarUI();
        }

        private GameObject GoldHudTarget()
        {
            if (goldHudRoot)
                return goldHudRoot;

            if (goldText)
                return goldText.transform?.parent ? goldText.transform.parent.gameObject : null;
        
            return null;
        }

        private void ApplyGoldHudVisibility()
        {
            var go = GoldHudTarget();
            if (go)
                go.SetActive(_goldHudRefCount > 0);
        }

        public void RequestShowGoldHud()
        {
            _goldHudRefCount++;
            ApplyGoldHudVisibility();
        }

        private void UpdateLevelUI()
        {
            if (playerLevelText != null)
                playerLevelText.text = $"Level: {Data.currentLevelIndex}";
        }

        private void UpdateAvatarUI()
        {
            if (avatarImage == null || allAvatars == null || allAvatars.Count == 0)
                return;

            var selectedAvatar = allAvatars.Find(a => a != null && a.avatarId == Data.avatarIndex);

            if (selectedAvatar != null && selectedAvatar.avatarSprite != null)
            {
                avatarImage.sprite = selectedAvatar.avatarSprite;
                avatarImage.enabled = true;
            }
            else
            {
                avatarImage.enabled = false;
            }
        }
        
        public void ReleaseShowGoldHud()
        {
            _goldHudRefCount = Mathf.Max(0, _goldHudRefCount - 1);
            ApplyGoldHudVisibility();
        }

        private void MigrateFromPlayerPrefsIfNeeded()
        {
            if (Data.playerName == "Jogador")
            {
                var legacyName = PlayerPrefs.GetString("PlayerName", "Jogador");
                if (!string.IsNullOrWhiteSpace(legacyName))
                    Data.playerName = legacyName;
            }

            if (Data.avatarIndex == 0)
            {
                var legacyAvatar = PlayerPrefs.GetInt("AvatarIndex", 0);
            }
        
            SaveManager.Instance.Save(Data);
        }

        private void ApplyProfileUI()
        {
            if (playerNameText)
                playerNameText.text = Data.playerName;
            
            if (playerIdText && !string.IsNullOrEmpty(Data.firebaseUserId))
                playerIdText.text = $"ID: {Data.firebaseUserId}";
        }

        private void UpdateGoldUI()
        {
            if (goldText) goldText.text = Data.gold.ToString();
            OnGoldChanged?.Invoke(Data.gold);
        }

        public void CopyUserIdToClipboard()
        {
            if (!string.IsNullOrEmpty(Data.firebaseUserId))
            {
                GUIUtility.systemCopyBuffer = Data.firebaseUserId;
                Debug.Log("ID copy to clipboard");
            }
        }

        private void Save()
        {
            SaveManager.Instance.Save(Data);
        }
    
        private void SaveAndSync()
        {
            Save();
            SyncToFirebase();
        }

        public void OpenProfile()  => profilePanel?.SetActive(true);
        public void CloseProfile() => profilePanel?.SetActive(false);

        public void ChangeAvatar()
        {
            SaveAndSync();
        }

        public void SelectAvatar(int avatarIndex)
        {
            Data.avatarIndex = avatarIndex;
            SaveAndSync();
        }

        private void ValidatePlayerName(string name)
        {
            if (!Regex.IsMatch(name, NameRegex))
            {
            }
            else
            {
                if (playerNameText)      playerNameText.text = name;
                Data.playerName = name;
                SaveAndSync();
            }
        }

        public void SetFirebaseUserId(string firebaseId)
        {
            if (!string.IsNullOrEmpty(firebaseId))
            {
                Data.firebaseUserId = firebaseId;
                
                if (playerIdText != null)
                    playerIdText.text = $"ID: {firebaseId}";
            }
        }

        public void SetPlayerName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            Data.playerName = name;
            if (playerNameText) playerNameText.text = name;
            SaveAndSync();
        }

        public bool CanAfford(int price) => Data.gold >= price;

        public void AddGold(int amount)
        {
            if(amount == 0)
                return;
        
            Data.gold = Mathf.Max(0, Data.gold + amount);
            UpdateGoldUI();
            SaveAndSync();
        }

        public bool TrySpendGold(int amount)
        {
            if(!CanAfford(amount))
            {
                Debug.LogWarning($"[PlayerProfileController] Cannot afford {amount} gold. Current balance: {Data.gold}");
                return false;
            }
        
            Data.gold -= amount;
            UpdateGoldUI();
            SaveAndSync();
            
            Debug.Log($"[PlayerProfileController] ✅ Spent {amount} gold. New balance: {Data.gold}");
            return true;
        }

        private void SyncToFirebase()
        {
            var dataSaver = FindObjectOfType<Managers.DataSaver>();
            if (dataSaver != null && dataSaver.dataToSave != null)
            {
                dataSaver.dataToSave.totalCoins = Data.gold;
                dataSaver.dataToSave.userName = Data.playerName;
                dataSaver.dataToSave.crrLevel = Data.currentLevelIndex;
                dataSaver.dataToSave.highScore = Data.highestLevelUnlocked;
                dataSaver.dataToSave.ownedCards = new List<string>(Data.ownedCards);
                dataSaver.dataToSave.equippedDeck = new List<string>(Data.equippedDeck);
                dataSaver.dataToSave.unlockedAvatar = new List<int>(Data.unlockedAvatars);
                dataSaver.dataToSave.purchasedAvatar = new List<int>(Data.purchasedAvatars);
                dataSaver.dataToSave.musicOn = Data.musicOn;
                dataSaver.dataToSave.sfxOn = Data.sfxOn;
                dataSaver.dataToSave.vfxOn = Data.vfxOn;
                dataSaver.dataToSave.language = Data.language;
        
                dataSaver.SaveData(force: true);
        
                Debug.Log($"[PlayerProfileController] ✅ Synced to Firebase - Coins: {Data.gold}, Unlocked Avatars: {Data.unlockedAvatars.Count}, Purchased Avatars: {Data.purchasedAvatars.Count}");
            }
            else
            {
                Debug.LogWarning("[PlayerProfileController] DataSaver not found or dataToSave is null!");
            }
        }

        public bool HasCard(string cardId)
            => !string.IsNullOrEmpty(cardId) && Data.ownedCards.Any(id => id == cardId);

        public bool TryPurchaseCard(string cardId, int price)
        {
            if (string.IsNullOrEmpty(cardId)) return false;
            if (HasCard(cardId)) return true;
            if (!TrySpendGold(price)) return false;

            Data.ownedCards.Add(cardId);
            SaveAndSync();
            return true;
        }

        public bool EquipToDeck(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return false;
            if (!HasCard(cardId)) return false;
            if (Data.equippedDeck.Contains(cardId)) return true;
            if (Data.equippedDeck.Count >= deckLimit) return false;

            Data.equippedDeck.Add(cardId);
            SaveAndSync();
            return true;
        }

        public bool UnequipFromDeck(string cardId)
        {
            if (!Data.equippedDeck.Contains(cardId)) return false;
            Data.equippedDeck.Remove(cardId);
            SaveAndSync();
            return true;
        }

        public void UnlockAvatar(int avatarId)
        {
            if (Data.unlockedAvatars.Contains(avatarId))
            {
                Debug.Log($"[PlayerProfileController] Avatar {avatarId} already unlocked");
                return;
            }
            
            Data.unlockedAvatars.Add(avatarId);
            SaveAndSync();
            
            Debug.Log($"[PlayerProfileController] ✅ Unlocked avatar {avatarId}. Total unlocked: {Data.unlockedAvatars.Count}");
        }

        public void PurchaseAvatar(int avatarId, int price)
        {
            if (Data.purchasedAvatars.Contains(avatarId))
            {
                Debug.Log($"[PlayerProfileController] Avatar {avatarId} already purchased");
                return;
            }
            
            if (!TrySpendGold(price))
            {
                Debug.LogWarning($"[PlayerProfileController] Not enough gold to purchase avatar {avatarId}. Need {price}, have {Data.gold}");
                return;
            }
        
            Data.purchasedAvatars.Add(avatarId);
            UnlockAvatar(avatarId);
            
            Debug.Log($"[PlayerProfileController] ✅ Purchased avatar {avatarId} for {price} gold. New balance: {Data.gold}");
        }

        public void SetMusicEnabled(bool enabled)
        {
            Data.musicOn = enabled;
            SaveAndSync();
        }

        public void SetSfxEnabled(bool enabled)
        {
            Data.sfxOn = enabled;
            SaveAndSync();
        }

        public void SetVfxEnabled(bool enabled)
        {
            Data.vfxOn = enabled;
            SaveAndSync();
        }

        public void SetLanguage(string language)
        {
            Data.language = language;
            SaveAndSync();
        }

        public void SetCurrentLevel(int levelIndex)
        {
            Data.currentLevelIndex = levelIndex;
            UpdateLevelUI();
            SaveAndSync();
        }

        public void SetHighestLevelUnlocked(int levelIndex)
        {
            if (levelIndex > Data.highestLevelUnlocked)
            {
                Data.highestLevelUnlocked = levelIndex;
                SaveAndSync();
            }
        }

        public void RegisterBestScore(string levelId, int score)
        {
            if (string.IsNullOrEmpty(levelId)) return;
        
            if (!Data.BestScores.ContainsKey(levelId))
            {
                Data.BestScores[levelId] = score;
            }
            else if (score > Data.BestScores[levelId])
            {
                Data.BestScores[levelId] = score;
            }
            else
            {
                return;
            }
        
            SaveAndSync();
        }

        public void SaveProfile()
        {
            SaveAndSync();
            UpdateAvatarUI();
        }

        public void AddGoldAndSave(int amount)
        {
            AddGold(amount);
        }

        public void OnLevelComplete(int levelIndex, int coinsEarned, int score, int stars)
        {
            AddGold(coinsEarned);
            SetCurrentLevel(levelIndex);
            SetHighestLevelUnlocked(levelIndex + 1);
            RegisterBestScore($"level_{levelIndex}", score);
        }

        public void SetRemoveAds(bool removeAds)
        {
            Data.removeAds = removeAds;
            SaveAndSync();
        }
    }
}
