using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DefaultNamespace
{
    [Obsolete("This class is deprecated. The new architecture uses VictoryView and DefeatView in New_GameplayCore.")]
    public class LevelCompletedPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _successUI;
        [SerializeField] private GameObject _failUI;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Sprite _winSprite;
        [SerializeField] private Sprite _loseSprite;
        [SerializeField] private GameObject _starsPanel;
        [SerializeField] private GameObject[] _stars;
        [SerializeField] private Button _nextPhaseButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _mainMenuButton;
        
        [Obsolete("Use new architecture instead")]
        private GameController_DEPRECATED _gameController;
        
        [Obsolete("Use new architecture instead")]
        private ScoreManager_DEPRECATED _scoreManager;

        public void Setup(bool isSuccess, int starCount = 0)
        {
            gameObject.SetActive(true);

            _successUI.SetActive(isSuccess);
            _failUI.SetActive(!isSuccess);

            _mainMenuButton.gameObject.SetActive(true);
            _mainMenuButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            if (isSuccess)
            {
                _titleText.text = "Congratulations!";
                _messageText.text = "You have completed the stage!";
                _iconImage.sprite = _winSprite;
                _starsPanel.SetActive(true);
                _nextPhaseButton.gameObject.SetActive(true);
                ShowStars(starCount);

                _nextPhaseButton.onClick.RemoveAllListeners();
                _nextPhaseButton.onClick.AddListener(NextPhase);
            }
            else
            {
                _titleText.text = "Time's up!";
                _messageText.text = "Not this time...";
                _iconImage.sprite = _loseSprite;
                _starsPanel.SetActive(false);
                _retryButton.gameObject.SetActive(true);

                _retryButton.onClick.RemoveAllListeners();
                _retryButton.onClick.AddListener(RetryPhase);
            }
        }

        [Obsolete("Use new architecture instead")]
        public void SetScoreManager(ScoreManager_DEPRECATED scoreManager)
        {
            _scoreManager = scoreManager;
        }

        [Obsolete("Use new architecture instead")]
        public void SetGameController(GameController_DEPRECATED gameController)
        {
            _gameController = gameController;
        }

        private void ShowStars(int starCount)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i].SetActive(i < starCount);
            }
        }

        private void HideAllStars()
        {
            foreach (var star in _stars)
            {
                star.SetActive(false);
            }
        }

        private void NextPhase()
        {
            gameObject.SetActive(false);

            if (_scoreManager != null)
            {
                _scoreManager.AdvancedToNextLevelExternally();
            }

            if (_gameController != null)
            {
                _gameController.StartNewPhase();
            }
        }

        private void RetryPhase()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
