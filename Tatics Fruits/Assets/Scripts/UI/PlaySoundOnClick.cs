using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class PlaySoundOnClick : MonoBehaviour
    {
        [SerializeField] private string soundName = "ButtonClick";

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(PlaySound);
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(PlaySound);
            }
        }

        private void PlaySound()
        {
            SoundManager.Instance.PlaySFX(soundName);
        }
    }
}
