using System.Collections.Generic;
using System.Linq;
using Core.ScriptableObjects;

namespace Core.Services
{
    public class AvatarService
    {
        private List<AvatarConfig> _allAvatars;
        private PlayerProfileService _profileService;
        
        public void Initialize(List<AvatarConfig> avatars, PlayerProfileService profile)
        {
            _allAvatars = avatars;
            _profileService = profile;
        }

        public bool IsAvatarUnlocked(int avatarId)
        {
            var avatar = _allAvatars.FirstOrDefault(a => a.avatarId == avatarId);
            if(avatar == null)
                return false;
            
            if(avatar.isDefault)
                return true;

            return _profileService.Data.unlockedAvatars.Contains(avatarId);
        }

        public bool CanPurchaseAvatar(int avatarId)
        {
            var avatar = _allAvatars.FirstOrDefault(a => a.avatarId == avatarId);
            if(avatar == null)
                return false;
            
            return _profileService.Data.gold >= avatar.unlockPrice && !IsAvatarUnlocked(avatarId);
        }

        public bool TryPurchaseAvatar(int avatarId)
        {
            var avatar = _allAvatars.FirstOrDefault(a => a.avatarId == avatarId);
            if (avatar == null || !CanPurchaseAvatar(avatarId))
                return false;
            
            _profileService.AddGold(-avatar.unlockPrice);
            _profileService.Data.unlockedAvatars.Add(avatarId);
            _profileService.Save();
            return true;
        }

        public void SelectAvatar(int avatarId)
        {
            if (!IsAvatarUnlocked(avatarId))
                return;
            
            _profileService.Data.avatarIndex = avatarId;
            _profileService.Save();
        }

        public List<AvatarConfig> GetAllAvatars() => _allAvatars;

        public AvatarConfig GetCurrentAvatar() => _allAvatars.FirstOrDefault(a => a.avatarId == _profileService.Data.avatarIndex);

    }
}