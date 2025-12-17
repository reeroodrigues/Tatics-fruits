using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Core.SaveSystem;

public class PlayerProfileController : MonoBehaviour
{
    [Header("UI / Perfil")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image avatarImage;
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
        
    }

    private void UpdateGoldUI()
    {
        if (goldText) goldText.text = Data.gold.ToString();
        OnGoldChanged?.Invoke(Data.gold);
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
            return false;
        
        Data.gold -= amount;
        UpdateGoldUI();
        SaveAndSync();
        return true;
    }

    private void SyncToFirebase()
    {
        var firebaseSync = FindObjectOfType<Core.Services.FirebaseProfileSyncService>();
        if (firebaseSync != null && firebaseSync.dataToSave != null)
        {
            firebaseSync.dataToSave.totalCoins = Data.gold;
            firebaseSync.dataToSave.userName = Data.playerName;
            firebaseSync.dataToSave.crrLevel = Data.currentLevelIndex;
            firebaseSync.dataToSave.highScore = Data.highestLevelUnlocked;
            firebaseSync.dataToSave.ownedCards = new System.Collections.Generic.List<string>(Data.ownedCards);
            firebaseSync.dataToSave.equippedDeck = new System.Collections.Generic.List<string>(Data.equippedDeck);
            firebaseSync.dataToSave.unlockedAvatar = new System.Collections.Generic.List<int>(Data.unlockedAvatars);
            firebaseSync.dataToSave.purchasedAvatar = new System.Collections.Generic.List<int>(Data.purchasedAvatars);
            firebaseSync.dataToSave.musicOn = Data.musicOn;
            firebaseSync.dataToSave.sfxOn = Data.sfxOn;
            firebaseSync.dataToSave.vfxOn = Data.vfxOn;
            firebaseSync.dataToSave.language = Data.language;
            firebaseSync.SaveData();
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
        if (Data.unlockedAvatars.Contains(avatarId)) return;
        Data.unlockedAvatars.Add(avatarId);
        SaveAndSync();
    }

    public void PurchaseAvatar(int avatarId, int price)
    {
        if (Data.purchasedAvatars.Contains(avatarId)) return;
        if (!TrySpendGold(price)) return;
        
        Data.purchasedAvatars.Add(avatarId);
        UnlockAvatar(avatarId);
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
        Save();
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
