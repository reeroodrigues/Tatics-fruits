using System;
using System.Collections.Generic;
using Core.ScriptableObjects;
using Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class ProfilePanelView : MonoBehaviour
    {
        [Header("UI References")] 
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI coinAmountText;
        [SerializeField] private TextMeshProUGUI lastLevelText;
        [SerializeField] private Image currentAvatarImage;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button deleteAccountButton;

        [Header("Avatar Scroll")] 
        [SerializeField] private Transform avatarGridContainer;
        [SerializeField] private GameObject avatarItemPrefab;

        [Header("Services")]
        [SerializeField] private List<AvatarConfig> allAvatars;
        
        [Header("Controller")]
        [SerializeField] private PlayerProfileController profileController;
        
        private PlayerProfileService _profileService;
        private AvatarService _avatarService;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);
            
            if (deleteAccountButton != null)
                deleteAccountButton.onClick.AddListener(OnDeleteAccountClick);
        }

        private void OnEnable()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }
            
            RefreshPanel();
        }
        
        private void RefreshPanel()
        {
            if (profileController != null && profileController.IsLoaded)
            {
                RefreshUI();
                PopulateAvatarGrid();
            }
        }

        private void OnDisable()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        public void Initialize(PlayerProfileService profile)
        {
            _profileService = profile;

            _avatarService = new AvatarService();
            _avatarService.Initialize(allAvatars, _profileService);

            RefreshUI();
            PopulateAvatarGrid();
        }

        private void RefreshUI()
        {
            if (profileController == null || profileController.Data == null)
                return;
            
            var data = profileController.Data;
            
            if (playerNameText != null)
                playerNameText.text = data.playerName;
            
            if (coinAmountText != null)
                coinAmountText.text = data.gold.ToString();
            
            if (lastLevelText != null)
                lastLevelText.text = $"Last Level: {data.currentLevelIndex + 1}";

            if (currentAvatarImage != null && profileController.Data.avatarIndex >= 0)
            {
                currentAvatarImage.enabled = true;
            }
        }

        private void PopulateAvatarGrid()
        {
            if (avatarGridContainer == null || avatarItemPrefab == null)
            {
                Debug.LogWarning("[ProfilePanelView] avatarGridContainer or avatarItemPrefab is null!");
                return;
            }
            
            if (allAvatars == null || allAvatars.Count == 0)
            {
                Debug.LogWarning("[ProfilePanelView] allAvatars list is empty! Assign AvatarConfig assets in the Inspector.");
                return;
            }
            
            foreach (Transform child in avatarGridContainer)
                Destroy(child.gameObject);

            Debug.Log($"[ProfilePanelView] Populating {allAvatars.Count} avatars");

            foreach (var avatarConfig in allAvatars)
            {
                if (avatarConfig == null)
                    continue;
                    
                var item = Instantiate(avatarItemPrefab, avatarGridContainer);
                var view = item.GetComponent<AvatarItemView>();
                
                if (view != null && profileController != null)
                {
                    bool isUnlocked = avatarConfig.isDefault || CheckIfAvatarUnlocked(avatarConfig.avatarId);
                    bool isSelected = profileController.Data.avatarIndex == avatarConfig.avatarId;
                    
                    Debug.Log($"[ProfilePanelView] Avatar {avatarConfig.avatarId}: isUnlocked={isUnlocked}, isSelected={isSelected}, isDefault={avatarConfig.isDefault}");
                    
                    view.Setup(avatarConfig, null, isUnlocked, isSelected);
                }
            }
        }
        
        private bool CheckIfAvatarUnlocked(int avatarId)
        {
            if (profileController == null || profileController.Data == null)
                return false;
            
            return profileController.Data.unlockedAvatars != null && 
                   profileController.Data.unlockedAvatars.Contains(avatarId);
        }

        private void OnDeleteAccountClick()
        {
            Debug.Log("Delete account requested");
        }

        private void ClosePanel()
        {
            if (profileController != null)
            {
                profileController.CloseProfile();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}