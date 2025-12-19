using System;
using System.Collections.Generic;
using Core.ScriptableObjects;
using Core.Services;
using Gameplay.Controllers;
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
        private AvatarServiceWrapper _avatarServiceWrapper;
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
            
            EnsureAvatarServiceCreated();
            RefreshPanel();
        }
        
        private void EnsureAvatarServiceCreated()
        {
            if (_avatarService == null && profileController != null && profileController.IsLoaded)
            {
                _avatarService = new AvatarService();
                
                _profileService = new PlayerProfileService();
                _profileService.Initialize(profileController.Data);
                
                _avatarService.Initialize(allAvatars, _profileService);
                
                _avatarServiceWrapper = new AvatarServiceWrapper(_avatarService, profileController, this);
                
                Debug.Log("[ProfilePanelView] AvatarService created on-demand");
            }
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
            
            _avatarServiceWrapper = new AvatarServiceWrapper(_avatarService, profileController, this);

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

            if (currentAvatarImage != null)
            {
                Debug.Log($"[ProfilePanelView] RefreshUI - Looking for avatar with ID: {data.avatarIndex}");
                Debug.Log($"[ProfilePanelView] Total avatars in list: {allAvatars?.Count ?? 0}");
                
                if (allAvatars != null && allAvatars.Count > 0)
                {
                    foreach (var avatar in allAvatars)
                    {
                        if (avatar != null)
                        {
                            Debug.Log($"[ProfilePanelView] Found avatar: ID={avatar.avatarId}, Sprite={(avatar.avatarSprite != null ? avatar.avatarSprite.name : "NULL")}");
                        }
                    }
                }
                
                var selectedAvatar = allAvatars?.Find(a => a != null && a.avatarId == data.avatarIndex);
                
                if (selectedAvatar != null)
                {
                    Debug.Log($"[ProfilePanelView] Selected avatar found: ID={selectedAvatar.avatarId}");
                    
                    if (selectedAvatar.avatarSprite != null)
                    {
                        currentAvatarImage.sprite = selectedAvatar.avatarSprite;
                        currentAvatarImage.enabled = true;
                        Debug.Log($"[ProfilePanelView] Avatar image set to: {selectedAvatar.avatarSprite.name}");
                    }
                    else
                    {
                        currentAvatarImage.enabled = false;
                        Debug.LogWarning($"[ProfilePanelView] Avatar {selectedAvatar.avatarId} has no sprite assigned!");
                    }
                }
                else
                {
                    currentAvatarImage.enabled = false;
                    Debug.LogWarning($"[ProfilePanelView] No avatar found with ID {data.avatarIndex}");
                }
            }
        }

        public void RefreshAvatarGrid()
        {
            if (profileController != null && profileController.IsLoaded)
            {
                RefreshUI();
                PopulateAvatarGrid();
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
                    
                    view.Setup(avatarConfig, _avatarServiceWrapper, isUnlocked, isSelected);
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
    
    public class AvatarServiceWrapper
    {
        private readonly AvatarService _avatarService;
        private readonly PlayerProfileController _controller;
        private readonly ProfilePanelView _view;

        public AvatarServiceWrapper(AvatarService avatarService, PlayerProfileController controller, ProfilePanelView view)
        {
            _avatarService = avatarService;
            _controller = controller;
            _view = view;
        }

        public void SelectAvatar(int avatarId)
        {
            if (_avatarService == null)
            {
                Debug.LogWarning("[AvatarServiceWrapper] AvatarService is null!");
                return;
            }

            _avatarService.SelectAvatar(avatarId);
            
            if (_controller != null)
                _controller.SaveProfile();
            
            if (_view != null)
                _view.RefreshAvatarGrid();
            
            Debug.Log($"[AvatarServiceWrapper] Selected avatar {avatarId}");
        }

        public bool TryPurchaseAvatar(int avatarId)
        {
            if (_avatarService == null)
            {
                Debug.LogWarning("[AvatarServiceWrapper] AvatarService is null!");
                return false;
            }

            bool success = _avatarService.TryPurchaseAvatar(avatarId);
            
            if (success)
            {
                if (_controller != null)
                    _controller.SaveProfile();
                
                if (_view != null)
                    _view.RefreshAvatarGrid();
                
                Debug.Log($"[AvatarServiceWrapper] Purchased avatar {avatarId}");
            }
            else
            {
                Debug.LogWarning($"[AvatarServiceWrapper] Failed to purchase avatar {avatarId}");
            }

            return success;
        }
    }
}
