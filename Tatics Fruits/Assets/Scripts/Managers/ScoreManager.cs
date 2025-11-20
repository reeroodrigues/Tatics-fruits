using System;
using DefaultNamespace;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Obsolete("This class is deprecated. Use New_GameplayCore.Services.ScoreService instead.")]
public class ScoreManager_DEPRECATED : MonoBehaviour
{
    private int _score;
    private HighScore _highScore;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private GameObject[] _stars;
    [SerializeField] private TextMeshProUGUI _levelText;

    [Header("Level Settings")]
    private readonly int[] _starThresholds = { 25, 50, 100 };
    private int _currentLevel = 1;
    public int CurrentLevel => _currentLevel;
    private int _scoreToNextLevel = 100;
    private int _targetScore;

    [Header("Level Completed Panels")]
    [SerializeField] private GameObject _victoryPanelPrefab;
    [SerializeField] private GameObject _defeatPanelPrefab;
    [SerializeField] private Transform _uiCanvas;
    [SerializeField] private Timer_DEPRECATED _timer;
    [SerializeField] private GameController_DEPRECATED _gameController;

    [Header("Score Animation")]
    [SerializeField] private float _scaleAmount = 1.3f;
    [SerializeField] private float _scaleDuration = 0.2f;

    private GameObject _currentPanel;
    private bool _hasEnded = false;

    public event Action OnLevelCompleted;

    private void Start()
    {
        _highScore = FindObjectOfType<HighScore>();
        
        _currentLevel = GameSession_DEPRECATED._currentLevel > 0 ? GameSession_DEPRECATED._currentLevel : 1;

        if (_targetScore == 0)
        {
            _targetScore = 100;
            Debug.LogWarning("Target Score não definido, atribuindo 100 por padrão.");
        }

        UpdateScoreUI();
        UpdateProgress();
        UpdateLevelUI();

        OnLevelCompleted += HandleEndOfRound;

        if (_timer != null)
        {
            _timer.OnRoundEnd += HandleEndOfRound;
        }
    }

    public void AddScore(int value)
    {
        _score += value;
        UpdateScoreUI();
        AnimateScore();
        UpdateProgress();
        CheckLevelProgression();
    }

    public int GetScore() => _score;

    public void SaveHighScore()
    {
        if (_highScore != null)
        {
            _highScore.TrySetHighScore(_score);
        }
    }

    public void ResetScore()
    {
        _score = 0;
        SaveHighScore();
        UpdateScoreUI();
        UpdateProgress();

        _scoreText.transform.DOScale(1.3f, 0.2f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        
    }

    private void UpdateScoreUI()
    {
        if (_scoreText != null)
            _scoreText.text = _score.ToString();
    }

    private void AnimateScore()
    {
        if (_scoreText == null) return;

        _scoreText.transform.DOScale(_scaleAmount, _scaleDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => _scoreText.transform.DOScale(1f, _scaleDuration));
    }

    private void UpdateProgress()
    {
        if (_progressBar == null || _targetScore == 0) return;

        float fillAmount = Mathf.Clamp((float)_score / _targetScore, 0f, 1f);
        _progressBar.DOValue(fillAmount, 0.5f);
        ActivateStars(_score);
    }

    private void ActivateStars(int score)
    {
        for (int i = 0; i < _stars.Length; i++)
        {
            if (i < _starThresholds.Length && score >= _starThresholds[i])
            {
                if (!_stars[i].activeSelf)
                {
                    _stars[i].SetActive(true);
                    AnimateStar(_stars[i]);
                }
            }
            else
            {
                _stars[i].SetActive(false);
            }
        }
    }

    public void SetTargetScore(int targetScore)
    {
        _targetScore = targetScore;
        _scoreToNextLevel = _targetScore;
        UpdateProgress();
    }

    private void CheckLevelProgression()
    {
        if (_score >= _targetScore)
        {
            OnLevelCompleted?.Invoke();
        }
    }

    public void HandleEndOfRound()
    {
        if (_hasEnded) return;

        _hasEnded = true;
        SaveHighScore();

        if (_timer != null)
        {
            _timer.StopTimer();
        }

        var victory = _score >= _targetScore;
        ShowLevelCompletedPanel(victory);
        
    }

    private void ShowLevelCompletedPanel(bool success)
    {
        if (_uiCanvas == null) return;

        if (_currentPanel != null)
        {
            Destroy(_currentPanel);
        }

        var panelPrefab = success ? _victoryPanelPrefab : _defeatPanelPrefab;

        if (panelPrefab == null) return;

        _currentPanel = Instantiate(panelPrefab, _uiCanvas);
        _currentPanel.SetActive(true);

        var panelController = _currentPanel.GetComponent<LevelCompletedPanel>();
        if (panelController != null)
        {
            var starsEarned = CalculateEarnedStars();
            panelController.Setup(success, success ? starsEarned : 0);

            panelController.SetGameController(_gameController);
            panelController.SetScoreManager(this);
        }
    }

    private int CalculateEarnedStars()
    {
        int count = 0;
        foreach (var threshold in _starThresholds)
        {
            if (_score >= threshold) count++;
        }
        return count;
    }

    private void AdvanceToNextLevel()
    {
        _currentLevel++;
        GameSession_DEPRECATED._currentLevel = _currentLevel;
        
        _scoreToNextLevel += Mathf.RoundToInt(_scoreToNextLevel * 0.5f);

        if (_progressBar != null)
            _progressBar.value = 0f;

        UpdateLevelUI();
        AnimateLevelUp();
    }

    private void UpdateLevelUI()
    {
        if (_levelText != null)
            _levelText.text = "Level " + _currentLevel;
    }

    private void AnimateLevelUp()
    {
        _levelText.transform.DOScale(1.5f, 0.3f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutBounce);
    }

    private void AnimateStar(GameObject star)
    {
        var image = star.GetComponent<Image>();
        if (image == null) return;

        star.transform.DORotate(new Vector3(0, 0, 15), 0.3f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        image.DOFade(1f, 0.2f)
            .From(0.5f)
            .SetLoops(2, LoopType.Yoyo);
    }

    public void AdvancedToNextLevelExternally()
    {
        _hasEnded = false;
        AdvanceToNextLevel();
    }
}
