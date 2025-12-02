using Core.ScriptableObjects;
using Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Views
{
    public class AvatarItemView : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;
        [SerializeField] private GameObject selectedBorder;
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject priceContainer;
        [SerializeField] private Button selectButton;
        [SerializeField] private TextMeshProUGUI priceText;

        private AvatarConfig _config;
        private AvatarServiceWrapper _avatarServiceWrapper;
        private bool _isUnlocked;
        private bool _isSelected;

        public void Setup(AvatarConfig avatarConfig, AvatarServiceWrapper avatarServiceWrapper, bool unlocked, bool selected)
        {
            _config = avatarConfig;
            _avatarServiceWrapper = avatarServiceWrapper;
            _isUnlocked = unlocked;
            _isSelected = selected;
            
            if (avatarImage != null && _config != null)
                avatarImage.sprite = _config.avatarSprite;
            
            if (selectedBorder != null)
            {
                selectedBorder.SetActive(_isSelected);
                Debug.Log($"[AvatarItemView] Avatar {_config?.avatarId} - Selected Border: {_isSelected}");
            }
            else
            {
                Debug.LogWarning("[AvatarItemView] selectedBorder is NULL!");
            }
            
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!_isUnlocked);
                Debug.Log($"[AvatarItemView] Avatar {_config?.avatarId} - Locked Overlay: {!_isUnlocked}");
            }
            else
            {
                Debug.LogWarning("[AvatarItemView] lockedOverlay is NULL!");
            }
            
            if (priceContainer != null)
            {
                bool showPrice = !_isUnlocked && _config != null && !_config.isDefault;
                priceContainer.SetActive(showPrice);
                Debug.Log($"[AvatarItemView] Avatar {_config?.avatarId} - Price Container: {showPrice}");
            }
            else
            {
                Debug.LogWarning("[AvatarItemView] priceContainer is NULL!");
            }

            if (priceText != null && _config != null)
                priceText.text = _config.unlockPrice.ToString();
            
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (_avatarServiceWrapper == null)
            {
                Debug.LogWarning("[AvatarItemView] AvatarServiceWrapper is null - cannot select avatar");
                return;
            }
            
            if(_isUnlocked)
            {
                Debug.Log($"[AvatarItemView] Selecting unlocked avatar {_config.avatarId}");
                _avatarServiceWrapper.SelectAvatar(_config.avatarId);
            }
            else
            {
                Debug.Log($"[AvatarItemView] Attempting to purchase avatar {_config.avatarId}");
                _avatarServiceWrapper.TryPurchaseAvatar(_config.avatarId);
            }
        }
    }
}
