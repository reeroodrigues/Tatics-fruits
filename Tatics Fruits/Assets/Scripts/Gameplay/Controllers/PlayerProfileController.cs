using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Serialization;

public class PlayerProfileController : MonoBehaviour
{
    [Header("UI / Perfil")]
    [SerializeField] private GameObject profilePanel;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Sprite[] avatars;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TextMeshProUGUI playerNameErrorText;
    [SerializeField] private GameObject avatarSelectionPanel;
    [SerializeField] private Button closeAvatarPanelButton;

    [Header("UI / Economia")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private GameObject goldHudRoot;
    [SerializeField] private bool goldHutVisibleByDefault = false;
    
    [Header("Deck")]
    [SerializeField] private int deckLimit = 5;

    private int goldHudRefCount = 0;
    private const string FileName = "player_profile.json";
    private const string NameRegex = @"^[A-Za-zÀ-ÖØ-öø-ÿ\s]+$";
    public bool IsLoaded { get; private set; }
    public event Action OnProfileLoaded;
    public event Action<int> OnGoldChanged;
    public PlayerProfileData Data { get; private set; } = new PlayerProfileData();

    private void Awake()
    {
        if (!JsonDataService.TryLoad<PlayerProfileData>(FileName, out var loaded))
        {
            Data = new PlayerProfileData();
            MigrateFromPlayerPrefsIfNeeded();
            Save();
        }
        else
        {
            Data = loaded ?? new PlayerProfileData();
            if (Data.ownedCards == null)    Data.ownedCards = new System.Collections.Generic.List<string>();
            if (Data.equippedDeck == null)  Data.equippedDeck = new System.Collections.Generic.List<string>();
            if (Data.daily == null)         Data.daily = new DailySystemData();
            if (Data.daily.login == null)   Data.daily.login = new DailyLoginData();
        }

        IsLoaded = true;
        OnProfileLoaded?.Invoke();

        goldHudRefCount = goldHutVisibleByDefault ? 1 : 0;
        ApplyGoldHudVisibility();
    }

    private void Start()
    {
        if (!JsonDataService.TryLoad<PlayerProfileData>(FileName, out var loaded))
        {
            Data = new PlayerProfileData();
            MigrateFromPlayerPrefsIfNeeded();
            Save();
        }
        else
        {
            Data = loaded ?? new PlayerProfileData();
            if (Data.ownedCards == null) Data.ownedCards = new System.Collections.Generic.List<string>();
            
            if (Data.equippedDeck == null) Data.equippedDeck = new System.Collections.Generic.List<string>();
        }
        
        ApplyProfileUI();
        UpdateGoldUI();
        
        if (playerNameInput != null)
            playerNameInput.onEndEdit.AddListener(ValidatePlayerName);
        
        if (closeAvatarPanelButton != null)
            closeAvatarPanelButton.onClick.AddListener(CloseAvatarSelection);
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
            go.SetActive(goldHudRefCount > 0);
    }

    public void RequestShowGoldHud()
    {
        goldHudRefCount++;
        ApplyGoldHudVisibility();
    }

    public void ReleaseShowGoldHud()
    {
        goldHudRefCount = Mathf.Max(0, goldHudRefCount - 1);
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
            Data.avatarIndex = Mathf.Clamp(legacyAvatar, 0, avatars != null && avatars.Length > 0 ? avatars.Length - 1 : 0);
        }
    }

    private void ApplyProfileUI()
    {
        if (playerNameText)
            playerNameText.text = Data.playerName;
        
        if (playerNameInput) 
            playerNameInput.text = Data.playerName;

        if (avatars != null && avatars.Length > 0 && avatarImage)
        {
            var idx = Mathf.Clamp(Data.avatarIndex, 0, avatars.Length - 1);
            avatarImage.sprite = avatars[idx];
        }
    }

    private void UpdateGoldUI()
    {
        if (goldText) goldText.text = Data.gold.ToString();
        OnGoldChanged?.Invoke(Data.gold);
    }

    private void Save() => JsonDataService.Save(FileName, Data);
    

    public void OpenProfile()  => profilePanel?.SetActive(true);
    public void CloseProfile() => profilePanel?.SetActive(false);

    public void OpenAvatarSelection()  => avatarSelectionPanel?.SetActive(true);
    public void CloseAvatarSelection() => avatarSelectionPanel?.SetActive(false);

    public void ChangeAvatar()
    {
        if (avatars == null || avatars.Length == 0) 
            return;
        
        Data.avatarIndex = (Data.avatarIndex + 1) % avatars.Length;
        avatarImage.sprite = avatars[Data.avatarIndex];
        Save();
    }

    public void SelectAvatar(int avatarIndex)
    {
        if (avatars == null || avatars.Length == 0) 
            return;
        
        Data.avatarIndex = Mathf.Clamp(avatarIndex, 0, avatars.Length - 1);
        avatarImage.sprite = avatars[Data.avatarIndex];
        Save();
        CloseAvatarSelection();
    }

    private void ValidatePlayerName(string name)
    {
        if (!Regex.IsMatch(name, NameRegex))
        {
            if (playerNameErrorText) playerNameErrorText.text = "Nome inválido! Apenas letras são permitidas.";
            if (playerNameInput)     playerNameInput.text = Data.playerName;
        }
        else
        {
            if (playerNameErrorText) playerNameErrorText.text = "";
            if (playerNameText)      playerNameText.text = name;
            Data.playerName = name;
            Save();
        }
    }

    public bool CanAfford(int price) => Data.gold >= price;

    public void AddGold(int amount)
    {
        if (amount == 0) return;
        Data.gold = Mathf.Max(0, Data.gold + amount);
        Save();
        UpdateGoldUI();
    }

    public bool TrySpendGold(int amount)
    {
        if (!CanAfford(amount)) return false;
        Data.gold -= amount;
        Save();
        UpdateGoldUI();
        return true;
    }

    public bool HasCard(string cardId)
        => !string.IsNullOrEmpty(cardId) && Data.ownedCards.Any(id => id == cardId);

    public bool TryPurchaseCard(string cardId, int price)
    {
        if (string.IsNullOrEmpty(cardId)) return false;
        if (HasCard(cardId)) return true;
        if (!TrySpendGold(price)) return false;

        Data.ownedCards.Add(cardId);
        Save();
        return true;
    }

    public bool EquipToDeck(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return false;
        if (!HasCard(cardId)) return false;
        if (Data.equippedDeck.Contains(cardId)) return true;
        if (Data.equippedDeck.Count >= deckLimit) return false;

        Data.equippedDeck.Add(cardId);
        Save();
        return true;
    }

    public bool UnequipFromDeck(string cardId)
    {
        if (!Data.equippedDeck.Contains(cardId)) return false;
        Data.equippedDeck.Remove(cardId);
        Save();
        return true;
    }

    public void SaveProfile()
    {
        JsonDataService.Save("player_profile.json", Data);
    }

    public void AddGoldAndSave(int amount)
    {
        if (amount == 0)
            return;
        
        Data.gold = Mathf.Max(0, Data.gold + amount);
        SaveProfile();
        UpdateGoldUI();
    }
}
