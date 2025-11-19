using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace New_GameplayCore.Views
{
    public class GameplaySettings : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button openSettingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button musicButton;
        [SerializeField] private Button sfxButton;
        [SerializeField] private Button quitButton;

        [Header("Sprites")]
        [SerializeField] private Sprite musicOnSprite;
        [SerializeField] private Sprite musicOffSprite;
        [SerializeField] private Sprite sfxOnSprite;
        [SerializeField] private Sprite sfxOffSprite;

        [Header("Quit Popup")]
        [SerializeField] private GameObject quitPopupPrefab;
        [SerializeField] private Transform canvasTransform;

        private const string MusicPrefKey = "MusicEnabled";
        private const string SfxPrefKey = "SFXEnabled";

        private bool _isMusicEnabled = true;
        private bool _isSfxEnabled = true;
        private Image _musicButtonImage;
        private Image _sfxButtonImage;

        private void Awake()
        {
            LoadSettings();
            InitializeComponents();
        }

        private void Start()
        {
            SetupButtonListeners();
            
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            UpdateButtonVisuals();
        }

        private void InitializeComponents()
        {
            if (musicButton != null)
            {
                _musicButtonImage = musicButton.GetComponent<Image>();
            }

            if (sfxButton != null)
            {
                _sfxButtonImage = sfxButton.GetComponent<Image>();
            }
        }

        private void SetupButtonListeners()
        {
            if (openSettingsButton != null)
            {
                openSettingsButton.onClick.AddListener(ToggleSettingsMenu);
            }

            if (musicButton != null)
            {
                musicButton.onClick.AddListener(ToggleMusic);
            }

            if (sfxButton != null)
            {
                sfxButton.onClick.AddListener(ToggleSFX);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitToMainMenu);
            }
        }

        private void ToggleSettingsMenu()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }

        private void ToggleMusic()
        {
            _isMusicEnabled = !_isMusicEnabled;
            SaveSettings();
            UpdateButtonVisuals();
        }

        private void ToggleSFX()
        {
            _isSfxEnabled = !_isSfxEnabled;
            SaveSettings();
            UpdateButtonVisuals();
        }

        private void UpdateButtonVisuals()
        {
            if (_musicButtonImage != null)
            {
                _musicButtonImage.sprite = _isMusicEnabled ? musicOnSprite : musicOffSprite;
            }

            if (_sfxButtonImage != null)
            {
                _sfxButtonImage.sprite = _isSfxEnabled ? sfxOnSprite : sfxOffSprite;
            }
        }

        private void QuitToMainMenu()
        {
            ShowQuitPopup();
        }

        private void ShowQuitPopup()
        {
            if (quitPopupPrefab != null)
            {
                Transform parentTransform = canvasTransform != null ? canvasTransform : transform.parent;
                Instantiate(quitPopupPrefab, parentTransform);
            }
        }

        private void LoadSettings()
        {
            _isMusicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) == 1;
            _isSfxEnabled = PlayerPrefs.GetInt(SfxPrefKey, 1) == 1;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetInt(MusicPrefKey, _isMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(SfxPrefKey, _isSfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (openSettingsButton != null)
            {
                openSettingsButton.onClick.RemoveListener(ToggleSettingsMenu);
            }

            if (musicButton != null)
            {
                musicButton.onClick.RemoveListener(ToggleMusic);
            }

            if (sfxButton != null)
            {
                sfxButton.onClick.RemoveListener(ToggleSFX);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitToMainMenu);
            }
        }
    }
}
