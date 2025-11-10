using UnityEngine;

namespace New_GameplayCore
{
    [CreateAssetMenu(menuName = "Tutorial/Slide", fileName = "Tutorial")]
    public class TutorialSlideSo : ScriptableObject
    {
        [Header("Visual")]
        public Sprite image;
        
        [Header("Texto")]
        [TextArea(2, 4)]
        public string description;
    }
}