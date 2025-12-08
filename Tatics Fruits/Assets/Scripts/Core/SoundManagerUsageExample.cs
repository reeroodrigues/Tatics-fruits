using UnityEngine;

namespace Core
{
    public class SoundManagerUsageExample : MonoBehaviour
    {
        private void Start()
        {
            PlayBackgroundMusic();
        }

        private void PlayBackgroundMusic()
        {
            SoundManager.Instance.PlayMusic("MainMenuMusic", fadeIn: true, fadeDuration: 1.5f);
        }

        public void OnButtonClick()
        {
            SoundManager.Instance.PlaySFX("ButtonClick");
        }

        public void OnCardFlip()
        {
            SoundManager.Instance.PlaySFX("CardFlip");
        }

        public void OnVictory()
        {
            SoundManager.Instance.PlaySFX("Victory");
        }

        public void SwitchToGameplayMusic()
        {
            SoundManager.Instance.StopMusic(fadeOut: true, fadeDuration: 1f);
            
            Invoke(nameof(PlayGameplayMusic), 1.1f);
        }

        private void PlayGameplayMusic()
        {
            SoundManager.Instance.PlayMusic("GameplayMusic", fadeIn: true, fadeDuration: 1.5f);
        }

        public void PlaySoundAtPosition(Vector3 position)
        {
            SoundManager.Instance.PlaySFXAtPoint("Explosion", position);
        }
    }
}
