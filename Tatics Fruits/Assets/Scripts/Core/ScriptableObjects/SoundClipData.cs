using UnityEngine;

namespace Core.ScriptableObjects
{
    public enum SoundCategory
    {
        Music,
        SFX,
        UI
    }
    [CreateAssetMenu(fileName = "SoundClipData", menuName = "Audio/SoundClipData")]
    public class SoundClipData : ScriptableObject
    {
        [Header("Audio Clip")]
        public AudioClip clip;
        public string soundName;
        
        [Header("Settings")]
        public SoundCategory category =  SoundCategory.Music;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3, 3f)] public float pitch = 1f;
        public bool loop = false;
        
        [Header("Pitch Variation")]
        public bool randomizePitch = false;
        [Range(0f, 0.5f)] public float pitchVariation = 0.1f;

        public float GetRandomPitch()
        {
            if (!randomizePitch)
                return pitch;
            
            return pitch + Random.Range(-pitchVariation, pitchVariation);
        }

    }
}