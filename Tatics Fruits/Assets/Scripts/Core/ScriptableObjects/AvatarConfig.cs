using UnityEngine;

namespace Core.ScriptableObjects
{
    [CreateAssetMenu(fileName = "AvatarConfig", menuName = "Game/Avatar Config")]
    public class AvatarConfig : ScriptableObject
    {
        public int avatarId;
        public Sprite avatarSprite;
        public string avatarName;
        public int unlockPrice;
        public bool isDefault;
    }
}